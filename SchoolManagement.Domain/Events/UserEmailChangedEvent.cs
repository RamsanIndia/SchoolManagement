// Domain/Events/UserEmailChangedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserEmailChangedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string NewEmail { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserEmailChangedEvent(Guid userId, string newEmail)
        {
            UserId = userId;
            NewEmail = newEmail;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
