// Domain/Events/UserActivatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserActivatedEvent : IDomainEvent
    {
        public Guid EventId => throw new NotImplementedException();

        public Guid UserId { get; }
        public string Username { get; }
        public DateTime OccurredOn { get; }

        public UserActivatedEvent(Guid userId, string username)
        {
            UserId = userId;
            Username = username;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
