using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services.Strategies
{
    public sealed class RoomConflictDetector : IConflictDetector
    {
        public ConflictDetail Detect(
            SlotAvailabilityRequest request,
            ConflictingEntries entries)
        {
            if (entries.RoomEntry == null)
                return null;

            var entry = entries.RoomEntry;
            return new ConflictDetail(
                ConflictType.Room,
                entry.Id,
                $"Room {request.RoomNumber} is already booked for another section",
                entry.TimePeriod.StartTime,
                entry.TimePeriod.EndTime,
                conflictingSectionId: entry.SectionId,
                conflictingTeacherId: entry.TeacherId,
                conflictingSubjectId: entry.SubjectId,
                conflictingRoomNumber: request.RoomNumber);
        }
    }
}
