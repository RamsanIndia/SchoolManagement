using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class TimeTableRepository : Repository<TimeTableEntry>, ITimeTableRepository
    {
        public TimeTableRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimeTableEntry>> GetBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(tt => tt.SectionId == sectionId && !tt.IsDeleted)
                .OrderBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.PeriodNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeTableEntry>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await _dbSet
                .Where(tt => tt.TeacherId == teacherId && !tt.IsDeleted)
                .Include(tt => tt.Section)
                    .ThenInclude(s => s.Class)
                .OrderBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.PeriodNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeTableEntry>> GetByDayAsync(Guid sectionId, DayOfWeek day)
        {
            return await _dbSet
                .Where(tt =>
                    tt.SectionId == sectionId &&
                    tt.DayOfWeek == day &&
                    !tt.IsDeleted)
                .OrderBy(tt => tt.PeriodNumber)
                .ToListAsync();
        }

        public async Task<bool> IsSlotAvailableAsync(Guid sectionId, DayOfWeek day, int periodNumber, CancellationToken cancellationToken, Guid? excludeId = null)
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

        public async Task<bool> IsTeacherAvailableAsync(Guid teacherId, DayOfWeek day, int periodNumber, CancellationToken cancellationToken, Guid? excludeId = null)
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
