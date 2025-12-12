// Domain/Events/UserPhoneVerifiedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserPhoneVerifiedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string PhoneNumber { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserPhoneVerifiedEvent(Guid userId, string phoneNumber)
        {
            UserId = userId;
            PhoneNumber = phoneNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
