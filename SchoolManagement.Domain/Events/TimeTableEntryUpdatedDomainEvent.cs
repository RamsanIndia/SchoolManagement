using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TimeTableEntryUpdatedDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid TimeTableEntryId { get; }
        public Guid SectionId { get; }
        public Guid OldSubjectId { get; }
        public Guid NewSubjectId { get; }
        public Guid OldTeacherId { get; }
        public Guid NewTeacherId { get; }
        public TimeSpan OldStartTime { get; }
        public TimeSpan OldEndTime { get; }
        public TimeSpan NewStartTime { get; }
        public TimeSpan NewEndTime { get; }
        public string OldRoomNumber { get; }
        public string NewRoomNumber { get; }
        public DateTime OccurredOn { get; }

        public TimeTableEntryUpdatedDomainEvent(
            Guid timeTableEntryId,
            Guid sectionId,
            Guid oldSubjectId,
            Guid newSubjectId,
            Guid oldTeacherId,
            Guid newTeacherId,
            TimeSpan oldStartTime,
            TimeSpan oldEndTime,
            TimeSpan newStartTime,
            TimeSpan newEndTime,
            string oldRoomNumber,
            string newRoomNumber)
        {
            TimeTableEntryId = timeTableEntryId;
            SectionId = sectionId;
            OldSubjectId = oldSubjectId;
            NewSubjectId = newSubjectId;
            OldTeacherId = oldTeacherId;
            NewTeacherId = newTeacherId;
            OldStartTime = oldStartTime;
            OldEndTime = oldEndTime;
            NewStartTime = newStartTime;
            NewEndTime = newEndTime;
            OldRoomNumber = oldRoomNumber;
            NewRoomNumber = newRoomNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
