// Domain/Events/UserPasswordChangedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserPasswordChangedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserPasswordChangedEvent(Guid userId, string username)
        {
            UserId = userId;
            Username = username;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
