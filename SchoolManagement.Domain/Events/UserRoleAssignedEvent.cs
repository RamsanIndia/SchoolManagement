// Domain/Events/UserRoleAssignedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserRoleAssignedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid UserId { get; }
        public Guid RoleId { get; }
        public DateTime? ExpiresAt { get; }
        public DateTime OccurredOn { get; }

        public UserRoleAssignedEvent(Guid userId, Guid roleId, DateTime? expiresAt)
        {
            EventId = Guid.NewGuid();
            UserId = userId;
            RoleId = roleId;
            ExpiresAt = expiresAt;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
