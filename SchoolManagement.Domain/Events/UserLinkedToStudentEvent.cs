// Domain/Events/UserLinkedToStudentEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserLinkedToStudentEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid StudentId { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId { get; } = Guid.NewGuid();

        public UserLinkedToStudentEvent(Guid userId, Guid studentId)
        {
            UserId = userId;
            StudentId = studentId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
