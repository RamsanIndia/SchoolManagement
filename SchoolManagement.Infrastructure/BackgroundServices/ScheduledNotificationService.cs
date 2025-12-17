using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service for processing scheduled notifications
    /// Checks for notifications that have reached their scheduled time
    /// </summary>
    public class ScheduledNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledNotificationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public ScheduledNotificationService(
            IServiceProvider serviceProvider,
            ILogger<ScheduledNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledNotificationsAsync(stoppingToken);
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in scheduled notification service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Scheduled Notification Service stopped");
        }

        private async Task ProcessScheduledNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            try
            {
                // Get pending notifications
                var pendingNotifications = await repository.GetPendingNotificationsAsync(
                    100, cancellationToken);

                // Count how many are now ready (scheduled time has passed)
                var nowReady = pendingNotifications
                    .Where(n => n.ScheduledAt.HasValue && n.IsReadyToSend())
                    .Count();

                if (nowReady > 0)
                {
                    _logger.LogInformation(
                        "{Count} scheduled notifications are now ready for processing",
                        nowReady);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking scheduled notifications");
            }
        }
    }
}
