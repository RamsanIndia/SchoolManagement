using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly SchoolManagementDbContext _context;

        public MenuRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        // Get single menu by ID
        public async Task<Menu> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Include(m => m.ParentMenu)
                .Include(m => m.SubMenus)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
        }

        // Get menu by name
        public async Task<Menu> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .FirstOrDefaultAsync(m => m.Name == name && !m.IsDeleted, cancellationToken);
        }

        // Get menus by role ID
        public async Task<IEnumerable<Menu>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Include(m => m.ParentMenu)
                .Include(m => m.SubMenus)
                .Where(m => !m.IsDeleted)
                .Where(m => m.RoleMenuPermissions.Any(rmp => rmp.RoleId == roleId && rmp.CanView))
                .OrderBy(m => m.SortOrder)
                .ToListAsync(cancellationToken);
        }

        // Get all menus
        public async Task<IEnumerable<Menu>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Include(m => m.ParentMenu)
                .Include(m => m.SubMenus)
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.ParentMenuId)
                .ThenBy(m => m.SortOrder)
                .ToListAsync(cancellationToken);
        }

        // Get active menus only
        public async Task<IEnumerable<Menu>> GetActiveMenusAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Include(m => m.ParentMenu)
                .Include(m => m.SubMenus)
                .Where(m => !m.IsDeleted && m.IsActive)
                .OrderBy(m => m.ParentMenuId)
                .ThenBy(m => m.SortOrder)
                .ToListAsync(cancellationToken);
        }

        // Get menus by parent ID
        public async Task<IEnumerable<Menu>> GetMenusByParentAsync(Guid? parentId, CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Where(m => m.ParentMenuId == parentId && !m.IsDeleted && m.IsActive)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(cancellationToken);
        }

        // Create new menu
        public async Task<Menu> CreateAsync(Menu menu, CancellationToken cancellationToken = default)
        {
            await _context.Menus.AddAsync(menu, cancellationToken);
            return menu;
        }

        // Update existing menu
        public async Task<Menu> UpdateAsync(Menu menu, CancellationToken cancellationToken = default)
        {
            _context.Menus.Update(menu);
            return await Task.FromResult(menu);
        }

        // Soft delete menu
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);

            if (menu != null)
            {
                menu.MarkAsDeleted();
                _context.Menus.Update(menu);
            }
        }

        // Get menu hierarchy (parent-child structure)
        public async Task<IEnumerable<Menu>> GetMenuHierarchyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Include(m => m.SubMenus.Where(sm => !sm.IsDeleted && sm.IsActive))
                .Where(m => !m.IsDeleted && m.IsActive && m.ParentMenuId == null)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Menu>> GetMenusByRoleAsync(string roleId)
        {
            if (!Guid.TryParse(roleId, out var roleGuid))
            {
                throw new ArgumentException("Invalid roleId format. Must be a Guid.", nameof(roleId));
            }

            return await _context.RoleMenuPermissions
                .Where(x => x.RoleId == roleGuid)
                .Select(x => x.Menu)
                .ToListAsync();
        }

    }
}