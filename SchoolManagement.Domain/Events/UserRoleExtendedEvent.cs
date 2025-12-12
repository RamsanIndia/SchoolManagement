// Domain/Events/UserRoleExtendedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserRoleExtendedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid UserId { get; }
        public Guid RoleId { get; }
        public DateTime? OldExpiryDate { get; }
        public DateTime NewExpiryDate { get; }
        public DateTime OccurredOn { get; }

        public UserRoleExtendedEvent(
            Guid userId,
            Guid roleId,
            DateTime? oldExpiryDate,
            DateTime newExpiryDate)
        {
            EventId = Guid.NewGuid();
            UserId = userId;
            RoleId = roleId;
            OldExpiryDate = oldExpiryDate;
            NewExpiryDate = newExpiryDate;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
