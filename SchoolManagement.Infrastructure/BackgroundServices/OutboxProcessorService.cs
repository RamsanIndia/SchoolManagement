// Infrastructure/BackgroundServices/OutboxProcessorService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Services;
using SchoolManagement.Persistence;
using SchoolManagement.Persistence.Outbox;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service that processes outbox messages and publishes them to Azure Service Bus
    /// Implements Transactional Outbox Pattern for reliable event delivery
    /// </summary>
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);
        private readonly int _batchSize = 20;
        private readonly int _maxRetries = 5;

        public OutboxProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxProcessorService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor Service started at {Time}", DateTime.UtcNow);

            // Wait for application to fully start before processing
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected when service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing outbox messages");
                }

                try
                {
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Outbox Processor Service stopped at {Time}", DateTime.UtcNow);
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope(); // ✅ FIXED: Changed from 'using' to 'await using'
            var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            // Create execution strategy correctly
            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                // Create NEW DbContext inside execution strategy for retry safety
                await using var retryScope = _scopeFactory.CreateAsyncScope(); // ✅ FIXED: Changed from 'using' to 'await using'
                var retryDbContext = retryScope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
                var retryPublisher = retryScope.ServiceProvider.GetRequiredService<IEventPublisher>();

                // Now safe to start transaction inside execution strategy
                await using var transaction = await retryDbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Get unprocessed messages
                    var messages = await retryDbContext.OutboxMessages
                        .Where(m => m.ProcessedAt == null && (m.RetryCount == null || m.RetryCount < _maxRetries))
                        .OrderBy(m => m.CreatedAt)
                        .Take(_batchSize)
                        .ToListAsync(cancellationToken);

                    if (!messages.Any())
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return;
                    }

                    _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

                    foreach (var message in messages)
                    {
                        await ProcessSingleMessageAsync(message, retryPublisher, cancellationToken);
                    }

                    // Save all changes in one transaction
                    await retryDbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Successfully processed {Count} outbox messages",
                        messages.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Failed to process outbox batch. Transaction rolled back.");
                    throw; // Let execution strategy retry
                }
            });
        }

        private async Task ProcessSingleMessageAsync(
            OutboxMessage message,
            IEventPublisher publisher,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Processing outbox message {MessageId} of type {EventType}",
                    message.Id,
                    message.EventType);

                // Publish to Azure Service Bus
                await publisher.PublishAsync(
                    message.EventType,
                    message.Payload,
                    cancellationToken);

                // Mark as processed
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation(
                    "Successfully published outbox message {MessageId} of type {EventType} with EventId {EventId}",
                    message.Id,
                    message.EventType,
                    message.EventId);
            }
            catch (Exception ex)
            {
                message.RetryCount = (message.RetryCount ?? 0) + 1; // ✅ FIXED: Added null-coalescing operator
                message.Error = TruncateError(ex.ToString(), 4000);

                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {MessageId} (Retry {RetryCount}/{MaxRetries}). EventType: {EventType}",
                    message.Id,
                    message.RetryCount,
                    _maxRetries,
                    message.EventType);

                // If max retries reached, mark as processed to prevent infinite retry
                if (message.RetryCount >= _maxRetries)
                {
                    message.ProcessedAt = DateTime.UtcNow; // Mark as processed to skip future attempts

                    _logger.LogCritical(
                        "Outbox message {MessageId} failed after {MaxRetries} retries. " +
                        "EventType: {EventType}. Marked as processed. Manual intervention required.",
                        message.Id,
                        _maxRetries,
                        message.EventType);
                }
            }
        }

        /// <summary>
        /// Truncate error message to fit database column
        /// </summary>
        private string TruncateError(string error, int maxLength)
        {
            if (string.IsNullOrEmpty(error) || error.Length <= maxLength)
                return error;

            return error.Substring(0, maxLength - 3) + "...";
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Outbox Processor Service is stopping. Checking remaining messages...");

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();

                var pendingCount = await dbContext.OutboxMessages
                    .CountAsync(m => m.ProcessedAt == null, cancellationToken);

                if (pendingCount > 0)
                {
                    _logger.LogWarning(
                        "{PendingCount} outbox messages remain unprocessed during shutdown",
                        pendingCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pending messages during shutdown");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
