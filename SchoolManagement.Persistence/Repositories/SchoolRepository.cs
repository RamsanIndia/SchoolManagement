// Persistence/Repositories/SchoolRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class SchoolRepository : Repository<School>, ISchoolRepository
    {
        public SchoolRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<School?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Schools
                .FirstOrDefaultAsync(s => s.Code == code && s.IsActive, cancellationToken);
        }

        /// ✅ FIXED: GetByCodeWithUsersAsync (EF Include)
        public async Task<School?> GetByCodeWithUsersAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Schools
                .Include(s => s.Users)  // ✅ EF Include (not property access)
                .FirstOrDefaultAsync(s => s.Code == code && s.IsActive, cancellationToken);
        }

        /// ✅ FIXED: Correct parameter order matching interface
        public async Task<List<School>> GetActiveSchoolsAsync(
            CancellationToken cancellationToken = default,
            SchoolType? type = null)
        {
            var query = _context.Schools.Where(s => s.IsActive);

            if (type.HasValue)
                query = query.Where(s => s.Type == type.Value);

            return await query
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<School>> GetSchoolsByLocationAsync(
            string city,
            string state,
            CancellationToken cancellationToken = default)
        {
            return await _context.Schools
                .Where(s => s.Address.Contains(city) &&
                          s.Address.Contains(state) &&
                          s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<SchoolWithTenantDto> GetSchoolWithTenantAsync(
        Guid schoolId,
        CancellationToken cancellationToken = default)
        {
            var result = await _context.Schools
                .IgnoreQueryFilters()
                .Where(s => s.Id == schoolId && !s.IsDeleted)
                .Select(s => new SchoolWithTenantDto
                {
                    SchoolId = s.Id,
                    SchoolCode = s.Code,
                    SchoolName = s.Name,
                    TenantId = (Guid)s.TenantId,
                    TenantCode = s.Tenant.Code,
                    TenantName = s.Tenant.Name
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}
