using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class WeeklyPeriodsUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid SectionSubjectId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public int PreviousPeriods { get; }
        public int NewPeriods { get; }

        public WeeklyPeriodsUpdatedEvent(Guid sectionSubjectId, Guid sectionId, Guid subjectId,
            int previousPeriods, int newPeriods)
        {
            SectionSubjectId = sectionSubjectId;
            SectionId = sectionId;
            SubjectId = subjectId;
            PreviousPeriods = previousPeriods;
            NewPeriods = newPeriods;
        }
    }
}
