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
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        private readonly SchoolManagementDbContext _context;

        public DepartmentRepository(SchoolManagementDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Department?> GetDepartmentWithTeachersAsync(
            Guid departmentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Departments
                .Include(d => d.Teachers)
                .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);
        }

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsDepartmentNameUniqueAsync(
            string name,
            Guid? excludeDepartmentId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Departments
                .Where(d => d.Name.ToLower() == name.ToLower());

            if (excludeDepartmentId.HasValue)
            {
                query = query.Where(d => d.Id != excludeDepartmentId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsDepartmentCodeUniqueAsync(
            string code,
            Guid? excludeDepartmentId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Departments
                .Where(d => d.Code.ToUpper() == code.ToUpper());

            if (excludeDepartmentId.HasValue)
            {
                query = query.Where(d => d.Id != excludeDepartmentId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<Department?> GetByCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(
                    d => d.Code.ToUpper() == code.ToUpper(),
                    cancellationToken
                );
        }
    }
}
