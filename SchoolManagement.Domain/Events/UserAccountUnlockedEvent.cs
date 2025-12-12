// Domain/Events/UserAccountUnlockedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserAccountUnlockedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public UserAccountUnlockedEvent(Guid userId, string username)
        {
            UserId = userId;
            Username = username;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
