using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Services.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services
{
    public sealed class SlotAvailabilityService : ISlotAvailabilityService
    {
        private readonly IConflictDetectionStrategy _conflictDetectionStrategy;

        public SlotAvailabilityService(
            IConflictDetectionStrategy conflictDetectionStrategy)
        {
            _conflictDetectionStrategy = conflictDetectionStrategy
                ?? throw new ArgumentNullException(nameof(conflictDetectionStrategy));
        }

        public SlotAvailabilityResult CheckAvailability(
            SlotAvailabilityRequest request,
            TimeTableEntry sectionEntry,
            TimeTableEntry teacherEntry,
            TimeTableEntry roomEntry)
        {
            // Create wrapper for conflicting entries
            var conflictingEntries = new ConflictingEntries(
                sectionEntry,
                teacherEntry,
                roomEntry);

            // Detect conflicts using strategy pattern
            var conflicts = _conflictDetectionStrategy
                .DetectConflicts(request, conflictingEntries);

            // Return result
            return conflicts.Any()
                ? SlotAvailabilityResult.Unavailable(conflicts)
                : SlotAvailabilityResult.Available();
        }
    }
}
