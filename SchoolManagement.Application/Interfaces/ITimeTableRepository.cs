using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ITimeTableRepository : IRepository<TimeTableEntry>
    {
        // Query methods - return entities for domain logic
        Task<TimeTableEntry> GetBySlotAsync(
            Guid sectionId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            CancellationToken cancellationToken = default);

        Task<TimeTableEntry> GetTeacherScheduleAsync(
            Guid teacherId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            CancellationToken cancellationToken = default);

        Task<TimeTableEntry> GetByRoomAndSlotAsync(
            string roomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeTableEntry>> GetBySectionIdAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeTableEntry>> GetByTeacherIdAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeTableEntry>> GetByDayAsync(
            Guid sectionId,
            DayOfWeek day,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeTableEntry>> GetSectionScheduleAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TimeTableEntry>> GetTeacherWeeklyScheduleAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default);

        // Conflict detection - returns entities instead of booleans
        Task<bool> HasScheduleConflictAsync(
            Guid sectionId,
            Guid teacherId,
            string roomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber,
            Guid? excludeEntryId = null,
            CancellationToken cancellationToken = default);

        // Legacy methods - marked for deprecation (return booleans)
        [Obsolete("Use GetBySlotAsync instead and check for null")]
        Task<bool> IsSlotAvailableAsync(
            Guid sectionId,
            DayOfWeek day,
            int periodNumber,
            CancellationToken cancellationToken = default,
            Guid? excludeId = null);

        [Obsolete("Use GetTeacherScheduleAsync instead and check for null")]
        Task<bool> IsTeacherAvailableAsync(
            Guid teacherId,
            DayOfWeek day,
            int periodNumber,
            CancellationToken cancellationToken = default,
            Guid? excludeId = null);
    }
}
