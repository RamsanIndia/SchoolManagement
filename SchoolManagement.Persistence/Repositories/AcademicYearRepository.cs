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
    public class AcademicYearRepository : Repository<AcademicYear>, IAcademicYearRepository
    {
        public AcademicYearRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<AcademicYear?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ay => ay.Name == name && !ay.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AcademicYear?> GetCurrentAcademicYearAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ay => ay.IsCurrent && !ay.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<AcademicYear>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ay => ay.IsActive && !ay.IsDeleted)
                .OrderByDescending(ay => ay.StartYear)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(ay => ay.Name == name && !ay.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(ay => ay.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<AcademicYear?> GetByIdWithClassesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ay => ay.Classes)
                .Where(ay => ay.Id == id && !ay.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<AcademicYear>> GetCurrentYearsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ay => ay.IsCurrent && !ay.IsDeleted)
                .ToListAsync(cancellationToken);
        }
    }
}
