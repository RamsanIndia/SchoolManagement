using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassTeacherAssignedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public Guid TeacherId { get; }
        public string AssignedBy { get; }

        public ClassTeacherAssignedEvent(Guid sectionId, Guid teacherId, string assignedBy)
        {
            SectionId = sectionId;
            TeacherId = teacherId;
            AssignedBy = assignedBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
