using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TimeTableEntryRescheduledDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid TimeTableEntryId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public Guid TeacherId { get; }
        public DayOfWeek OldDayOfWeek { get; }
        public DayOfWeek NewDayOfWeek { get; }
        public int OldPeriodNumber { get; }
        public int NewPeriodNumber { get; }
        public TimeSpan OldStartTime { get; }
        public TimeSpan OldEndTime { get; }
        public TimeSpan NewStartTime { get; }
        public TimeSpan NewEndTime { get; }
        public DateTime OccurredOn { get; }

        public TimeTableEntryRescheduledDomainEvent(
            Guid timeTableEntryId,
            Guid sectionId,
            Guid subjectId,
            Guid teacherId,
            DayOfWeek oldDayOfWeek,
            DayOfWeek newDayOfWeek,
            int oldPeriodNumber,
            int newPeriodNumber,
            TimeSpan oldStartTime,
            TimeSpan oldEndTime,
            TimeSpan newStartTime,
            TimeSpan newEndTime)
        {
            TimeTableEntryId = timeTableEntryId;
            SectionId = sectionId;
            SubjectId = subjectId;
            TeacherId = teacherId;
            OldDayOfWeek = oldDayOfWeek;
            NewDayOfWeek = newDayOfWeek;
            OldPeriodNumber = oldPeriodNumber;
            NewPeriodNumber = newPeriodNumber;
            OldStartTime = oldStartTime;
            OldEndTime = oldEndTime;
            NewStartTime = newStartTime;
            NewEndTime = newEndTime;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
