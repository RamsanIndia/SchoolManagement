using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service for cleaning up old notifications
    /// Archives or deletes notifications older than retention period
    /// </summary>
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);
        private readonly int _retentionDays = 90; // Keep notifications for 90 days

        public NotificationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<NotificationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Cleanup Service started");

            // Wait before first cleanup
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldNotificationsAsync(stoppingToken);
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification cleanup service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Notification Cleanup Service stopped");
        }

        private async Task CleanupOldNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();

            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

                // Delete or archive old delivered/cancelled notifications
                var oldNotifications = await dbContext.Notifications
                    .Where(n => n.CreatedAt < cutoffDate)
                    .Where(n => n.Status == NotificationStatus.Delivered ||
                               n.Status == NotificationStatus.Cancelled)
                    .ToListAsync(cancellationToken);

                if (oldNotifications.Any())
                {
                    _logger.LogInformation(
                        "Cleaning up {Count} notifications older than {Days} days",
                        oldNotifications.Count, _retentionDays);

                    // Option 1: Delete
                    dbContext.Notifications.RemoveRange(oldNotifications);

                    // Option 2: Archive to separate table (recommended)
                    // await ArchiveNotificationsAsync(oldNotifications, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Cleanup completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old notifications");
            }
        }
    }
}
