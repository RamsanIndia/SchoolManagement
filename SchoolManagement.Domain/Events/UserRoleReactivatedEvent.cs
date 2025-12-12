// Domain/Events/UserRoleReactivatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserRoleReactivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid UserId { get; }
        public Guid RoleId { get; }
        public DateTime? NewExpiresAt { get; }
        public DateTime OccurredOn { get; }

        public UserRoleReactivatedEvent(Guid userId, Guid roleId, DateTime? newExpiresAt)
        {
            EventId = Guid.NewGuid();
            UserId = userId;
            RoleId = roleId;
            NewExpiresAt = newExpiresAt;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
