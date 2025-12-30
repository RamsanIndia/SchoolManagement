using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class TimeTableRepository : Repository<TimeTableEntry>, ITimeTableRepository
    {
        public TimeTableRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<TimeTableEntry> GetBySlotAsync(
            Guid sectionId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.SectionId == sectionId &&
                         t.DayOfWeek == dayOfWeek &&
                         t.PeriodNumber == periodNumber &&
                         !t.IsDeleted,
                    cancellationToken);
        }

        public async Task<TimeTableEntry> GetTeacherScheduleAsync(
            Guid teacherId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.TeacherId == teacherId &&
                         t.DayOfWeek == dayOfWeek &&
                         t.PeriodNumber == periodNumber &&
                         !t.IsDeleted,
                    cancellationToken);
        }

        public async Task<TimeTableEntry> GetByRoomAndSlotAsync(
            string roomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber,
            CancellationToken cancellationToken = default)
        {
            var normalizedRoom = roomNumber.Trim().ToUpperInvariant();

            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.RoomNumber.Value == normalizedRoom &&
                         t.DayOfWeek == dayOfWeek &&
                         t.PeriodNumber == periodNumber &&
                         !t.IsDeleted,
                    cancellationToken);
        }

        public async Task<IEnumerable<TimeTableEntry>> GetBySectionIdAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(tt => tt.SectionId == sectionId && !tt.IsDeleted)
                .OrderBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.PeriodNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeTableEntry>> GetByTeacherIdAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(tt => tt.TeacherId == teacherId && !tt.IsDeleted)
                .Include(tt => tt.Section)
                    .ThenInclude(s => s.Class)
                .OrderBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.PeriodNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeTableEntry>> GetByDayAsync(
            Guid sectionId,
            DayOfWeek day,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(tt =>
                    tt.SectionId == sectionId &&
                    tt.DayOfWeek == day &&
                    !tt.IsDeleted)
                .OrderBy(tt => tt.PeriodNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeTableEntry>> GetSectionScheduleAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(tt => tt.Section)
                .Where(tt => tt.SectionId == sectionId && !tt.IsDeleted)
                .OrderBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.PeriodNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeTableEntry>> GetTeacherWeeklyScheduleAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(tt => tt.TeacherId == teacherId && !tt.IsDeleted)
                .OrderBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.PeriodNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasScheduleConflictAsync(
            Guid sectionId,
            Guid teacherId,
            string roomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber,
            Guid? excludeEntryId = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedRoom = roomNumber.Trim().ToUpperInvariant();

            var query = _dbSet
                .Where(tt => !tt.IsDeleted &&
                           tt.DayOfWeek == dayOfWeek &&
                           tt.PeriodNumber == periodNumber &&
                           (tt.SectionId == sectionId ||
                            tt.TeacherId == teacherId ||
                            tt.RoomNumber.Value == normalizedRoom));

            if (excludeEntryId.HasValue)
            {
                query = query.Where(tt => tt.Id != excludeEntryId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        // Legacy methods - kept for backward compatibility
        public async Task<bool> IsSlotAvailableAsync(
            Guid sectionId,
            DayOfWeek day,
            int periodNumber,
            CancellationToken cancellationToken = default,
            Guid? excludeId = null)
        {
            var query = _dbSet.Where(tt =>
                tt.SectionId == sectionId &&
                tt.DayOfWeek == day &&
                tt.PeriodNumber == periodNumber &&
                !tt.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(tt => tt.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsTeacherAvailableAsync(
            Guid teacherId,
            DayOfWeek day,
            int periodNumber,
            CancellationToken cancellationToken = default,
            Guid? excludeId = null)
        {
            var query = _dbSet.Where(tt =>
                tt.TeacherId == teacherId &&
                tt.DayOfWeek == day &&
                tt.PeriodNumber == periodNumber &&
                !tt.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(tt => tt.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }
    }
}
