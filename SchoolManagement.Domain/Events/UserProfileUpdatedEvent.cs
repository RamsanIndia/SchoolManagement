// Domain/Events/UserProfileUpdatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserProfileUpdatedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserProfileUpdatedEvent(Guid userId, string firstName, string lastName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
