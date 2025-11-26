using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IMenuRepository
    {
        Task<Menu> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Menu> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Menu>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Menu>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Menu>> GetActiveMenusAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Menu>> GetMenusByParentAsync(Guid? parentId, CancellationToken cancellationToken = default);
        Task<Menu> CreateAsync(Menu menu, CancellationToken cancellationToken = default);
        Task<Menu> UpdateAsync(Menu menu, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Menu>> GetMenuHierarchyAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<Menu>> GetMenusByRoleAsync(string roleId);

    }
}