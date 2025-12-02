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
    public class SectionRepository : Repository<Section>, ISectionRepository
    {
        public SectionRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<Section> GetByIdWithDetailsAsync(Guid id,CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(s => s.Class)
                .Include(s => s.SectionSubjects)
                .Include(s => s.TimeTableEntries)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<IEnumerable<Section>> GetByClassIdAsync(Guid classId,CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(s => s.ClassId == classId && !s.IsDeleted)
                .Include(s => s.Class)
                .Include(s => s.SectionSubjects)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Section>> GetActiveSectionsAsync(CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(s => s.IsActive && !s.IsDeleted)
                .Include(s => s.Class)
                .OrderBy(s => s.Class.Grade)
                .ThenBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSectionNameExistsAsync(Guid classId, string sectionName, Guid? excludeId = null)
        {
            var query = _dbSet.Where(s =>
                s.ClassId == classId &&
                s.Name == sectionName &&
                !s.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Section>> GetSectionsByTeacherAsync(Guid teacherId)
        {
            return await _dbSet
                .Where(s => s.ClassTeacherId == teacherId && !s.IsDeleted)
                .Include(s => s.Class)
                .OrderBy(s => s.Class.Grade)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<int> GetStudentCountAsync(Guid sectionId)
        {
            var section = await _dbSet.FindAsync(sectionId);
            return section?.CurrentStrength ?? 0;
        }
    }
}
