using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TimeTableEntryCreatedDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid TimeTableEntryId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public Guid TeacherId { get; }
        public DayOfWeek DayOfWeek { get; }
        public int PeriodNumber { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public string RoomNumber { get; }
        public DateTime OccurredOn { get; }

        public TimeTableEntryCreatedDomainEvent(
            Guid timeTableEntryId,
            Guid sectionId,
            Guid subjectId,
            Guid teacherId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            TimeSpan startTime,
            TimeSpan endTime,
            string roomNumber)
        {
            TimeTableEntryId = timeTableEntryId;
            SectionId = sectionId;
            SubjectId = subjectId;
            TeacherId = teacherId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            StartTime = startTime;
            EndTime = endTime;
            RoomNumber = roomNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
