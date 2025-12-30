using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TimeTableEntryCancelledDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid TimeTableEntryId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public Guid TeacherId { get; }
        public DayOfWeek DayOfWeek { get; }
        public int PeriodNumber { get; }
        public DateTime OccurredOn { get; }

        public TimeTableEntryCancelledDomainEvent(
            Guid timeTableEntryId,
            Guid sectionId,
            Guid subjectId,
            Guid teacherId,
            DayOfWeek dayOfWeek,
            int periodNumber)
        {
            TimeTableEntryId = timeTableEntryId;
            SectionId = sectionId;
            SubjectId = subjectId;
            TeacherId = teacherId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
