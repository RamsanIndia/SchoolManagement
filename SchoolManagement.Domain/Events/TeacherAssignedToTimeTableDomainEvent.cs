using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherAssignedToTimeTableDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid TimeTableEntryId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public Guid OldTeacherId { get; }
        public Guid NewTeacherId { get; }
        public DayOfWeek DayOfWeek { get; }
        public int PeriodNumber { get; }
        public DateTime OccurredOn { get; }

        public TeacherAssignedToTimeTableDomainEvent(
            Guid timeTableEntryId,
            Guid sectionId,
            Guid subjectId,
            Guid oldTeacherId,
            Guid newTeacherId,
            DayOfWeek dayOfWeek,
            int periodNumber)
        {
            TimeTableEntryId = timeTableEntryId;
            SectionId = sectionId;
            SubjectId = subjectId;
            OldTeacherId = oldTeacherId;
            NewTeacherId = newTeacherId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
