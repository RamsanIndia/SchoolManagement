using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class RoomChangedForTimeTableEntryDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid TimeTableEntryId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public string OldRoomNumber { get; }
        public string NewRoomNumber { get; }
        public DayOfWeek DayOfWeek { get; }
        public int PeriodNumber { get; }
        public DateTime OccurredOn { get; }

        public RoomChangedForTimeTableEntryDomainEvent(
            Guid timeTableEntryId,
            Guid sectionId,
            Guid subjectId,
            string oldRoomNumber,
            string newRoomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber)
        {
            TimeTableEntryId = timeTableEntryId;
            SectionId = sectionId;
            SubjectId = subjectId;
            OldRoomNumber = oldRoomNumber;
            NewRoomNumber = newRoomNumber;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
