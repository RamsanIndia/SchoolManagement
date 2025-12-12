// Domain/Events/UserPhoneNumberChangedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserPhoneNumberChangedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string NewPhoneNumber { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();  // Implement this

        public UserPhoneNumberChangedEvent(Guid userId, string newPhoneNumber)
        {
            UserId = userId;
            NewPhoneNumber = newPhoneNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
