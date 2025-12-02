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
    public class ClassRepository : Repository<Class>, IClassRepository
    {
        public ClassRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<Class> GetByIdWithSectionsAsync(Guid id,CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(c => c.Sections)
                    .ThenInclude(s => s.SectionSubjects)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<IEnumerable<Class>> GetByAcademicYearAsync(Guid academicYearId)
        {
            return await _dbSet
                .Where(c => c.AcademicYearId == academicYearId && !c.IsDeleted)
                .Include(c => c.Sections)
                .OrderBy(c => c.Grade)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetActiveClassesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive && !c.IsDeleted)
                .Include(c => c.Sections)
                .OrderBy(c => c.Grade)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> IsClassCodeExistsAsync(string classCode, CancellationToken cancellationToken, Guid? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Code == classCode && !c.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<(IEnumerable<Class> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string searchTerm,
            bool? isActive)
        {
            var query = _dbSet
                .Where(c => !c.IsDeleted)
                .Include(c => c.Sections)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.Name.Contains(searchTerm) ||
                    c.Code.Contains(searchTerm) ||
                    c.Description.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Grade)
                .ThenBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

       
    }
}
