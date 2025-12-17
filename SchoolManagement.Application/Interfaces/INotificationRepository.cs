using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Notification> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int batchSize = 100, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetFailedNotificationsForRetryAsync(int batchSize = 100, CancellationToken cancellationToken = default);
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
    }
}
