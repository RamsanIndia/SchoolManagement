using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services.Strategies
{
    public sealed class SectionConflictDetector : IConflictDetector
    {
        public ConflictDetail Detect(
            SlotAvailabilityRequest request,
            ConflictingEntries entries)
        {
            if (entries.SectionEntry == null)
                return null;

            var entry = entries.SectionEntry;
            return new ConflictDetail(
                ConflictType.Section,
                entry.Id,
                $"Section already has a class scheduled with teacher {entry.TeacherId}",
                entry.TimePeriod.StartTime,
                entry.TimePeriod.EndTime,
                conflictingSectionId: request.SectionId,
                conflictingTeacherId: entry.TeacherId,
                conflictingSubjectId: entry.SubjectId,
                conflictingRoomNumber: entry.RoomNumber.Value);
        }
    }
}
