using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class RoleRepository : Repository<Role>,IRoleRepository
    {
        public RoleRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public IQueryable<Role> GetQueryable()
        {
            return _context.Roles.Where(r => !r.IsDeleted);
        }

        public async Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Roles
                .Include(r => r.RoleMenuPermissions)
                .ThenInclude(rmp => rmp.Menu)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        }

        public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name && !r.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Level)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetActiveRolesAsync()
        {
            return await _context.Roles
                .Where(r => !r.IsDeleted && r.IsActive)
                .OrderBy(r => r.Level)
                .ToListAsync();
        }

        public async Task<Role> CreateAsync(Role role)
        {
            _context.Roles.Add(role);
            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            return role;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var role = await GetByIdAsync(id, cancellationToken);
            if (role != null && !role.IsSystemRole)
            {
                role.MarkAsDeleted();
                _context.Roles.Update(role);
            }
        }
    }
}
