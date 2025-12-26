// Infrastructure/BackgroundServices/OutboxProcessorService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Services;
using SchoolManagement.Domain.Common;
using SchoolManagement.Infrastructure.EventBus;
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
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventBus>();

            // Use execution strategy properly - don't mix with explicit transactions
            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                // Get unprocessed messages (no transaction needed for read)
                var messages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedAt == null && (m.RetryCount == null || m.RetryCount < _maxRetries))
                    .OrderBy(m => m.CreatedAt)
                    .Take(_batchSize)
                    .AsNoTracking() //  Use no tracking for read
                    .ToListAsync(cancellationToken);

                if (!messages.Any())
                {
                    _logger.LogDebug("No outbox messages to process");
                    return;
                }

                _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

                // ✅ Re-attach entities to the context for tracking
                foreach (var message in messages)
                {
                    dbContext.OutboxMessages.Attach(message);
                }

                // Process each message
                foreach (var message in messages)
                {
                    await ProcessSingleMessageAsync(message, publisher, cancellationToken);
                }

                // ✅ CRITICAL: Save all changes in one batch
                try
                {
                    var savedCount = await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Successfully processed and saved {Count} outbox messages",
                        savedCount);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Concurrency conflict updating outbox messages. Will retry on next cycle.");
                    // Don't throw - let it retry on next cycle
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save outbox message updates to database");
                    throw; // Execution strategy will retry
                }
            });
        }

        private async Task ProcessSingleMessageAsync(
            OutboxMessage message,
            IEventBus publisher,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Processing outbox message {MessageId} of type {EventType}",
                    message.Id,
                    message.EventType);

                // Validate payload
                if (string.IsNullOrWhiteSpace(message.Payload))
                {
                    _logger.LogError("Payload is null or empty for message {MessageId}", message.Id);
                    message.Error = "Payload is null or empty";
                    message.ProcessedAt = DateTime.UtcNow;
                    message.RetryCount = _maxRetries;
                    return; // ✅ EF Core tracks this change, will be saved
                }

                // Load event type
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogError(
                        "Could not load type {EventType} for message {MessageId}. " +
                        "Ensure the type name is fully qualified with assembly.",
                        message.EventType,
                        message.Id);
                    message.Error = $"Type '{message.EventType}' not found";
                    message.ProcessedAt = DateTime.UtcNow;
                    message.RetryCount = _maxRetries;
                    return; // ✅ EF Core tracks this change
                }

                // Verify type implements IDomainEvent
                if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
                {
                    _logger.LogError(
                        "Type {EventType} does not implement IDomainEvent for message {MessageId}",
                        message.EventType,
                        message.Id);
                    message.Error = $"Type '{message.EventType}' does not implement IDomainEvent";
                    message.ProcessedAt = DateTime.UtcNow;
                    message.RetryCount = _maxRetries;
                    return; // ✅ EF Core tracks this change
                }

                // Deserialize
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };

                var deserializedObject = JsonSerializer.Deserialize(message.Payload, eventType, jsonOptions);

                if (deserializedObject == null)
                {
                    _logger.LogError(
                        "Deserialization returned null for {EventType} message {MessageId}. Payload: {Payload}",
                        message.EventType,
                        message.Id,
                        message.Payload.Length > 500 ? message.Payload.Substring(0, 500) + "..." : message.Payload);
                    message.Error = "Deserialization returned null";
                    message.ProcessedAt = DateTime.UtcNow;
                    message.RetryCount = _maxRetries;
                    return; // ✅ EF Core tracks this change
                }

                // Cast to IDomainEvent
                var domainEvent = deserializedObject as IDomainEvent;
                if (domainEvent == null)
                {
                    _logger.LogError(
                        "Failed to cast deserialized object to IDomainEvent. " +
                        "Actual type: {ActualType}, Expected type: {EventType}, MessageId: {MessageId}",
                        deserializedObject.GetType().FullName,
                        message.EventType,
                        message.Id);
                    message.Error = $"Object of type '{deserializedObject.GetType().FullName}' cannot be cast to IDomainEvent";
                    message.ProcessedAt = DateTime.UtcNow;
                    message.RetryCount = _maxRetries;
                    return; // ✅ EF Core tracks this change
                }

                // ✅ Publish to Service Bus using reflection
                var publishMethod = publisher.GetType()
                    .GetMethod(nameof(IEventBus.PublishAsync))
                    ?.MakeGenericMethod(eventType);

                if (publishMethod == null)
                {
                    throw new InvalidOperationException($"Could not find PublishAsync method on {publisher.GetType().Name}");
                }

                await (Task)publishMethod.Invoke(publisher, new object[] { domainEvent, cancellationToken });

                // ✅ Mark as successfully processed
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
                // Don't increment RetryCount on success - leave it as is for audit trail

                _logger.LogInformation(
                    "Successfully published outbox message {MessageId} of type {EventType} with EventId {EventId}",
                    message.Id,
                    message.EventType,
                    domainEvent.EventId);
            }
            catch (JsonException jsonEx)
            {
                // Permanent error - deserialization failed
                message.RetryCount = _maxRetries;
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = TruncateError($"JSON Deserialization Error: {jsonEx.Message}", 4000);

                _logger.LogError(
                    jsonEx,
                    "JSON deserialization failed for outbox message {MessageId}. " +
                    "EventType: {EventType}. Payload: {Payload}",
                    message.Id,
                    message.EventType,
                    message.Payload?.Length > 500 ? message.Payload.Substring(0, 500) + "..." : message.Payload);
            }
            catch (Exception ex)
            {
                // Transient error - increment retry count
                message.RetryCount = (message.RetryCount ?? 0) + 1;
                message.Error = TruncateError(ex.ToString(), 4000);
                // ✅ Don't set ProcessedAt yet - allow retry

                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {MessageId} (Retry {RetryCount}/{MaxRetries}). " +
                    "EventType: {EventType}. Error: {Error}",
                    message.Id,
                    message.RetryCount,
                    _maxRetries,
                    message.EventType,
                    ex.Message);

                // Mark as processed if max retries reached
                if (message.RetryCount >= _maxRetries)
                {
                    message.ProcessedAt = DateTime.UtcNow;

                    _logger.LogCritical(
                        "Outbox message {MessageId} failed after {MaxRetries} retries. " +
                        "EventType: {EventType}. Marked as processed. Manual intervention required. " +
                        "Last Error: {Error}",
                        message.Id,
                        _maxRetries,
                        message.EventType,
                        message.Error);
                }
            }
            // ✅ All changes tracked by EF Core - will be saved in ProcessOutboxMessagesAsync
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
                        "{PendingCount} outbox messages remain unprocessed during shutdown. " +
                        "They will be processed on next startup.",
                        pendingCount);
                }
                else
                {
                    _logger.LogInformation("All outbox messages processed successfully before shutdown");
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