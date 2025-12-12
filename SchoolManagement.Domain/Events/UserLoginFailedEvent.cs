// Domain/Events/UserLoginFailedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserLoginFailedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid UserId { get; }
        public string Username { get; }
        public int FailedAttempts { get; }
        public DateTime OccurredOn { get; }

        public UserLoginFailedEvent(Guid userId, string username, int failedAttempts)
        {
            UserId = userId;
            Username = username;
            FailedAttempts = failedAttempts;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
