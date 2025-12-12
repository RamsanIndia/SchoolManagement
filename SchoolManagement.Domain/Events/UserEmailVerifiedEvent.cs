// Domain/Events/UserEmailVerifiedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserEmailVerifiedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserEmailVerifiedEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
