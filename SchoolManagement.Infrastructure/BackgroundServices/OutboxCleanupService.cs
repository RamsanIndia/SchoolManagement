using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Cleans up old processed outbox messages
    /// </summary>
    public class OutboxCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run daily
        private readonly int _retentionDays = 30; // Keep messages for 30 days

        public OutboxCleanupService(
            IServiceProvider serviceProvider,
            ILogger<OutboxCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up outbox messages");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Outbox Cleanup Service stopped");
        }

        private async Task CleanupOldMessages(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

            var oldMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldMessages.Any())
            {
                dbContext.OutboxMessages.RemoveRange(oldMessages);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Cleaned up {Count} old outbox messages older than {CutoffDate}",
                    oldMessages.Count,
                    cutoffDate
                );
            }
        }
    }
}
