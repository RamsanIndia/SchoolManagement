using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services.Strategies
{
    public sealed class TeacherConflictDetector : IConflictDetector
    {
        public ConflictDetail Detect(
            SlotAvailabilityRequest request,
            ConflictingEntries entries)
        {
            if (entries.TeacherEntry == null)
                return null;

            var entry = entries.TeacherEntry;
            return new ConflictDetail(
                ConflictType.Teacher,
                entry.Id,
                $"Teacher is already teaching another section in Room {entry.RoomNumber.Value}",
                entry.TimePeriod.StartTime,
                entry.TimePeriod.EndTime,
                conflictingSectionId: entry.SectionId,
                conflictingTeacherId: request.TeacherId,
                conflictingSubjectId: entry.SubjectId,
                conflictingRoomNumber: entry.RoomNumber.Value);
        }
    }
}
