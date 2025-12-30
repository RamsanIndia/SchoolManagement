using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SectionSubjectTeacherChangedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid SectionSubjectId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public Guid PreviousTeacherId { get; }
        public Guid NewTeacherId { get; }

        public SectionSubjectTeacherChangedEvent(Guid sectionSubjectId, Guid sectionId,
            Guid subjectId, Guid previousTeacherId, Guid newTeacherId)
        {
            SectionSubjectId = sectionSubjectId;
            SectionId = sectionId;
            SubjectId = subjectId;
            PreviousTeacherId = previousTeacherId;
            NewTeacherId = newTeacherId;
        }
    }
}
