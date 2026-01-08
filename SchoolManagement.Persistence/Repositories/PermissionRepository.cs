using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        public PermissionRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public IQueryable<Permission> GetQueryable()
        {
            return _context.Permissions.Where(p => !p.IsDeleted);
        }

        public async Task<Permission> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        }

        public async Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == name && !p.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .Where(p => p.Module == module && !p.IsDeleted)
                .OrderBy(p => p.Action)
                .ToListAsync(cancellationToken);
        }

        public async Task<Permission> AddAsync(Permission permission, CancellationToken cancellationToken)
        {
            await _context.Permissions.AddAsync(permission, cancellationToken);
            return permission;
        }

        public Task<Permission> UpdateAsync(Permission permission, CancellationToken cancellationToken)
        {
            _context.Permissions.Update(permission);
            return Task.FromResult(permission);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var permission = await GetByIdAsync(id, cancellationToken);
            if (permission != null && !permission.IsSystemPermission)
            {
                permission.MarkAsDeleted();
                _context.Permissions.Update(permission);
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<Permission, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .Where(p => !p.IsDeleted)
                .AnyAsync(predicate, cancellationToken);
        }
    }
}
