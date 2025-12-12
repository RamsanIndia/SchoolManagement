// Domain/Events/UserAccountLockedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserAccountLockedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime LockedUntil { get; }
        public int FailedAttempts { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public UserAccountLockedEvent(Guid userId, string username, DateTime lockedUntil, int failedAttempts)
        {
            UserId = userId;
            Username = username;
            LockedUntil = lockedUntil;
            FailedAttempts = failedAttempts;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
