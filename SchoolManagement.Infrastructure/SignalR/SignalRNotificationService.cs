using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.SignalR
{
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendToUserAsync(
            string userId,
            NotificationDto notification,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                _logger.LogInformation(
                    "Notification sent via SignalR to user. UserId: {UserId}, NotificationId: {NotificationId}",
                    userId, notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending notification via SignalR. UserId: {UserId}, NotificationId: {NotificationId}",
                    userId, notification.Id);
            }
        }

        public async Task SendToGroupAsync(
            string groupName,
            NotificationDto notification,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                _logger.LogInformation(
                    "Notification sent via SignalR to group. Group: {GroupName}, NotificationId: {NotificationId}",
                    groupName, notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending notification via SignalR. Group: {GroupName}, NotificationId: {NotificationId}",
                    groupName, notification.Id);
            }
        }

        public async Task SendToAllAsync(
            NotificationDto notification,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .All
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                _logger.LogInformation(
                    "Notification sent via SignalR to all clients. NotificationId: {NotificationId}",
                    notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error broadcasting notification via SignalR. NotificationId: {NotificationId}",
                    notification.Id);
            }
        }

        public async Task SendStatusUpdateAsync(
            Guid notificationId,
            NotificationStatus status,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var statusUpdate = new
                {
                    NotificationId = notificationId,
                    Status = status,
                    StatusText = status.ToString(),
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("NotificationStatusChanged", statusUpdate, cancellationToken);

                _logger.LogInformation(
                    "Status update sent via SignalR. NotificationId: {NotificationId}, Status: {Status}",
                    notificationId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending status update via SignalR. NotificationId: {NotificationId}",
                    notificationId);
            }
        }
    }
}
