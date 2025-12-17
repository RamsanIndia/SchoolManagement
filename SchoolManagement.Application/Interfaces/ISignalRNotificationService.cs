using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ISignalRNotificationService
    {
        Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default);
        Task SendToGroupAsync(string groupName, NotificationDto notification, CancellationToken cancellationToken = default);
        Task SendToAllAsync(NotificationDto notification, CancellationToken cancellationToken = default);
        Task SendStatusUpdateAsync(Guid notificationId, NotificationStatus status, string userId, CancellationToken cancellationToken = default);
    }
}
