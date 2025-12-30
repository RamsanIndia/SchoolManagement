using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SubjectAssignedToSectionEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public Guid TeacherId { get; }
        public int WeeklyPeriods { get; }
        public bool IsMandatory { get; }

        public SubjectAssignedToSectionEvent(Guid sectionId, Guid subjectId, Guid teacherId,
            int weeklyPeriods, bool isMandatory)
        {
            SectionId = sectionId;
            SubjectId = subjectId;
            TeacherId = teacherId;
            WeeklyPeriods = weeklyPeriods;
            IsMandatory = isMandatory;
        }
    }
}
