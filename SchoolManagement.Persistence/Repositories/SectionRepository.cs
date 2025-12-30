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

        public async Task<bool> IsSectionNameExistsAsync(
            Guid classId,
            string sectionName,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(s => s.ClassId == classId &&
                              s.Name.ToLower() == sectionName.ToLower(),
                         cancellationToken);
        }

        public async Task<bool> IsSectionNameExistsExceptAsync(
            Guid classId,
            string sectionName,
            Guid exceptSectionId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(s => s.ClassId == classId &&
                              s.Name.ToLower() == sectionName.ToLower() &&
                              s.Id != exceptSectionId,
                         cancellationToken);
        }

        public async Task<Section?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Class)
                .Include(s => s.Students)
                .Include(s => s.SectionSubjects)
                .Include(s => s.TimeTableEntries)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<int> GetActiveSectionCountByClassAsync(
            Guid classId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .CountAsync(s => s.ClassId == classId && s.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Section>> GetSectionsByClassIdAsync(
            Guid classId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Section?> GetSectionByClassTeacherIdAsync(
    Guid teacherId,
    CancellationToken cancellationToken = default)
        {
            return await _context.Sections
                .Include(s => s.Class)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.ClassTeacherId == teacherId && s.IsActive,
                    cancellationToken
                );
        }

        public async Task<IEnumerable<Section>> GetSectionsByClassIdWithTeachersAsync(
            Guid classId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Sections
                .Include(s => s.Class)
                .Where(s => s.ClassId == classId && s.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
