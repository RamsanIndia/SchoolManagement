using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.SignalR
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly INotificationRepository _repository;

        public NotificationHub(
            ILogger<NotificationHub> logger,
            INotificationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value ??
                         Context.User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                _logger.LogInformation(
                    "Client connected to NotificationHub. ConnectionId: {ConnectionId}, UserId: {UserId}",
                    Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value ??
                         Context.User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

                _logger.LogInformation(
                    "Client disconnected from NotificationHub. ConnectionId: {ConnectionId}, UserId: {UserId}",
                    Context.ConnectionId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client can subscribe to specific notification types
        public async Task SubscribeToChannel(string channel)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel_{channel}");

            _logger.LogInformation(
                "Client subscribed to channel. ConnectionId: {ConnectionId}, Channel: {Channel}",
                Context.ConnectionId, channel);
        }

        public async Task UnsubscribeFromChannel(string channel)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel_{channel}");

            _logger.LogInformation(
                "Client unsubscribed from channel. ConnectionId: {ConnectionId}, Channel: {Channel}",
                Context.ConnectionId, channel);
        }

        // Get notification history
        public async Task<IEnumerable<NotificationDto>> GetRecentNotifications(int count = 20)
        {
            var userId = Context.User?.FindFirst("sub")?.Value ??
                         Context.User?.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Enumerable.Empty<NotificationDto>();
            }

            // This would need to be implemented in repository
            // For now, returning empty
            await Task.CompletedTask;
            return Enumerable.Empty<NotificationDto>();
        }

        // Mark notification as read
        public async Task MarkAsRead(Guid notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);

            if (notification != null)
            {
                // Add read timestamp to metadata or create a separate tracking mechanism
                _logger.LogInformation(
                    "Notification marked as read. NotificationId: {NotificationId}",
                    notificationId);
            }
        }
    }
}
