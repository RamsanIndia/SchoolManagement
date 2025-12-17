using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service for retrying failed notifications with exponential backoff
    /// </summary>
    public class RetryFailedNotificationsService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RetryFailedNotificationsService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(5);
        private readonly int _batchSize = 25;

        public RetryFailedNotificationsService(
            IServiceProvider serviceProvider,
            ILogger<RetryFailedNotificationsService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Retry Failed Notifications Service started");

            // Initial delay before starting
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RetryFailedNotificationsAsync(stoppingToken);
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in retry failed notifications service");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }

            _logger.LogInformation("Retry Failed Notifications Service stopped");
        }

        private async Task RetryFailedNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                var failedNotifications = await repository
                    .GetFailedNotificationsForRetryAsync(_batchSize, cancellationToken);

                var eligibleForRetry = failedNotifications
                    .Where(n => n.CanRetry() && ShouldRetryNow(n))
                    .ToList();

                if (!eligibleForRetry.Any())
                {
                    return;
                }

                _logger.LogInformation(
                    "Retrying {Count} failed notifications",
                    eligibleForRetry.Count);

                foreach (var notification in eligibleForRetry)
                {
                    try
                    {
                        //var command = new RetryFailedNotificationCommand(notification.Id);
                        //var result = await mediator.Send(command, cancellationToken);

                        //if (result.IsSuccess)
                        //{
                        //    _logger.LogInformation(
                        //        "Notification {NotificationId} queued for retry (Attempt {RetryCount})",
                        //        notification.Id, notification.RetryCount + 1);
                        //}
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error retrying notification {NotificationId}",
                            notification.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RetryFailedNotificationsAsync");
            }
        }

        private bool ShouldRetryNow(Notification notification)
        {
            // Exponential backoff: 2^retryCount minutes
            var delayMinutes = Math.Pow(2, notification.RetryCount);
            var nextRetryTime = notification.CreatedAt.AddMinutes(delayMinutes);
            return DateTime.UtcNow >= nextRetryTime;
        }
    }
}
