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
    /// Background service for collecting and logging notification metrics
    /// Useful for monitoring and alerting
    /// </summary>
    public class NotificationMetricsService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationMetricsService> _logger;
        private readonly TimeSpan _metricsInterval = TimeSpan.FromMinutes(5);

        public NotificationMetricsService(
            IServiceProvider serviceProvider,
            ILogger<NotificationMetricsService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Metrics Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectMetricsAsync(stoppingToken);
                    await Task.Delay(_metricsInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification metrics service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Notification Metrics Service stopped");
        }

        private async Task CollectMetricsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();

            try
            {
                var now = DateTime.UtcNow;
                var lastHour = now.AddHours(-1);

                var metrics = await dbContext.Notifications
                    .Where(n => n.CreatedAt >= lastHour)
                    .GroupBy(n => n.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var pendingCount = await dbContext.Notifications
                    .CountAsync(n => n.Status == NotificationStatus.Pending, cancellationToken);

                _logger.LogInformation(
                    "Notification Metrics - Pending: {Pending}, Last Hour: {Metrics}",
                    pendingCount,
                    string.Join(", ", metrics.Select(m => $"{(NotificationStatus)m.Status}: {m.Count}")));

                // Alert if pending count is too high
                if (pendingCount > 1000)
                {
                    _logger.LogWarning(
                        "High pending notification count detected: {Count}. System may be backlogged.",
                        pendingCount);
                }

                // Alert if too many failures in last hour
                var failedCount = metrics.FirstOrDefault(m => m.Status == NotificationStatus.Failed)?.Count ?? 0;
                if (failedCount > 100)
                {
                    _logger.LogWarning(
                        "High failure rate detected: {Count} failures in last hour",
                        failedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting notification metrics");
            }
        }
    }
}
