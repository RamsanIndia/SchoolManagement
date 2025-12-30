using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SubjectAssignedToTeacherEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public Guid SubjectId { get; }
        public bool IsPrimary { get; }

        public SubjectAssignedToTeacherEvent(Guid teacherId, Guid subjectId, bool isPrimary)
        {
            TeacherId = teacherId;
            SubjectId = subjectId;
            IsPrimary = isPrimary;
        }
    }
}
