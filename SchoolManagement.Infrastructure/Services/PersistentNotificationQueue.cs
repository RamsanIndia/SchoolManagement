using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services
{
    public class PersistentNotificationQueue : INotificationQueue
    {
        private readonly SchoolManagementDbContext _context;
        private readonly SemaphoreSlim _signal;

        public PersistentNotificationQueue(SchoolManagementDbContext context)
        {
            _context = context;
            _signal = new SemaphoreSlim(0);
        }

        public bool IsEmpty => !_context.Set<Notification>()
            .Any(n => n.Status == NotificationStatus.Pending);

        public async Task EnqueueAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            // Don't set Status here (setter is private); ensure callers enqueue a Pending notification.
            // If you must enforce Pending, add a domain method like ResetToPending() inside Notification.
            _context.Set<Notification>().Add(notification);

            await _context.SaveChangesAsync(cancellationToken);

            _signal.Release();
        }

        public async Task<Notification?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            await _signal.WaitAsync(cancellationToken);

            var notification = await _context.Set<Notification>()
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (notification != null)
            {
                // Use domain behavior instead of setting Status directly.
                var result = notification.MarkAsProcessing();
                if (!result.Status)
                    return null; // or throw; depending on your policy

                await _context.SaveChangesAsync(cancellationToken);
            }

            return notification;
        }

        public async Task<IEnumerable<Notification>> DequeueBatchAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            var notifications = await _context.Set<Notification>()
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            // Mark each as processing via domain method (no direct setter access).
            foreach (var notification in notifications)
            {
                var result = notification.MarkAsProcessing();
                if (!result.Status)
                    continue; // or handle failure (throw/log) based on your needs
            }

            await _context.SaveChangesAsync(cancellationToken);
            return notifications;
        }

        public async Task<int> GetQueueCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Notification>()
                .CountAsync(n => n.Status == NotificationStatus.Pending, cancellationToken);
        }

        public Task MarkAsProcessedAsync(Guid notificationId)
        {
            throw new NotImplementedException();
        }

        public Task MarkAsFailedAsync(Guid notificationId, string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
