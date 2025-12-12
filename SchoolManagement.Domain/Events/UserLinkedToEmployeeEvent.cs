// Domain/Events/UserLinkedToEmployeeEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserLinkedToEmployeeEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid EmployeeId { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserLinkedToEmployeeEvent(Guid userId, Guid employeeId)
        {
            UserId = userId;
            EmployeeId = employeeId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
