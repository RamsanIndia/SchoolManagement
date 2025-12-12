// Domain/Events/UserRoleRevokedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserRoleRevokedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid UserId { get; }
        public Guid RoleId { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; }

        public UserRoleRevokedEvent(Guid userId, Guid roleId, string reason)
        {
            EventId = Guid.NewGuid();
            UserId = userId;
            RoleId = roleId;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
