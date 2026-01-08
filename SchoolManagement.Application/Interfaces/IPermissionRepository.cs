using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PermissionEntity = SchoolManagement.Domain.Entities.Permission;

namespace SchoolManagement.Application.Interfaces
{
    public interface IPermissionRepository: IRepository<Permission>
    {
        IQueryable<Permission> GetQueryable();
        Task<PermissionEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PermissionEntity> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<PermissionEntity>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<PermissionEntity>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);
        Task<PermissionEntity> AddAsync(PermissionEntity permission, CancellationToken cancellationToken);
        Task<PermissionEntity> UpdateAsync(PermissionEntity permission, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Expression<Func<PermissionEntity, bool>> predicate, CancellationToken cancellationToken = default);
    }
}