using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service that continuously processes pending notifications
    /// Integrates with Clean Architecture and DDD patterns
    /// </summary>
    public class NotificationProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationProcessorService> _logger;
        private readonly TimeSpan _processingInterval;
        private readonly int _batchSize;
        private readonly int _maxConcurrentProcessing;

        public NotificationProcessorService(
            IServiceProvider serviceProvider,
            ILogger<NotificationProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Configurable intervals - can be moved to appsettings.json
            _processingInterval = TimeSpan.FromSeconds(5);
            _batchSize = 50; // Process 50 notifications at a time
            _maxConcurrentProcessing = 10; // Max 10 concurrent sends
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Notification Processor Service started. Interval: {Interval}s, BatchSize: {BatchSize}",
                _processingInterval.TotalSeconds, _batchSize);

            // Wait a bit before starting to ensure database is ready
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotificationBatchAsync(stoppingToken);
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Notification Processor Service cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error in notification processor service");
                    // Wait longer on critical errors to avoid rapid failure loops
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Notification Processor Service stopped gracefully");
        }

        private async Task ProcessNotificationBatchAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                // Get pending notifications that are ready to send
                var pendingNotifications = await repository.GetPendingNotificationsAsync(
                    _batchSize, cancellationToken);

                var readyToSend = pendingNotifications
                    .Where(n => n.IsReadyToSend())
                    .ToList();

                if (!readyToSend.Any())
                {
                    return; // No notifications to process
                }

                _logger.LogInformation(
                    "Processing {Count} notifications from queue (Total pending: {Total})",
                    readyToSend.Count, pendingNotifications.Count());

                // Process notifications with limited concurrency
                var semaphore = new SemaphoreSlim(_maxConcurrentProcessing);
                var tasks = readyToSend.Select(async notification =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await ProcessSingleNotificationAsync(
                            notification.Id, mediator, cancellationToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                _logger.LogInformation("Completed processing batch of {Count} notifications", readyToSend.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification batch");
            }
        }

        private async Task ProcessSingleNotificationAsync(
            Guid notificationId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            try
            {
                //var command = new ProcessNotificationCommand(notificationId);
                //var result = await mediator.Send(command, cancellationToken);

                //if (result.IsFailure)
                //{
                //    _logger.LogWarning(
                //        "Failed to process notification {NotificationId}: {Error}",
                //        notificationId, result.Error);
                //}
                //else
                //{
                //    _logger.LogDebug(
                //        "Successfully processed notification {NotificationId}",
                //        notificationId);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception processing notification {NotificationId}",
                    notificationId);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Processor Service is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}
