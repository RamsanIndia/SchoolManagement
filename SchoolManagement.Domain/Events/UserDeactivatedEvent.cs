// Domain/Events/UserDeactivatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserDeactivatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid UserId { get; }
        public string Username { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; }

        public UserDeactivatedEvent(Guid userId, string username, string reason)
        {
            UserId = userId;
            Username = username;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
