using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    /// <summary>
    /// Domain event raised when a class teacher is changed in a section
    /// </summary>
    public class ClassTeacherChangedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public Guid PreviousTeacherId { get; }
        public Guid NewTeacherId { get; }
        public string ChangedBy { get; }

        public ClassTeacherChangedEvent(
            Guid sectionId,
            Guid previousTeacherId,
            Guid newTeacherId,
            string changedBy)
        {
            SectionId = sectionId;
            PreviousTeacherId = previousTeacherId;
            NewTeacherId = newTeacherId;
            ChangedBy = changedBy;
        }
    }
}
