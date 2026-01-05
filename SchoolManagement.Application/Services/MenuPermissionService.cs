using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class MenuPermissionService : IMenuPermissionService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IRoleMenuPermissionRepository _roleMenuPermissionRepository;
        private readonly IUserService _userService;

        public MenuPermissionService(
            IMenuRepository menuRepository,
            IRoleMenuPermissionRepository roleMenuPermissionRepository,
            IUserService userService)
        {
            _menuRepository = menuRepository;
            _roleMenuPermissionRepository = roleMenuPermissionRepository;
            _userService = userService;
        }

        public async Task<Result<IEnumerable<MenuItemDto>>> GetUserMenusAsync(Guid userId)
        {
            try
            {
                // ✅ GetUserRolesAsync returns IEnumerable<Role>, not Result<IEnumerable<Role>>
                var userRoles = await _userService.GetUserRolesAsync(userId);

                if (userRoles == null || !userRoles.Any())
                {
                    return Result<IEnumerable<MenuItemDto>>.Success(
                        new List<MenuItemDto>(),
                        "No active roles found for user"
                    );
                }

                var userRoleIds = userRoles.Select(r => r.Id).ToList();

                // Get menu hierarchy
                var allMenus = await _menuRepository.GetMenuHierarchyAsync();
                var userMenus = new List<MenuItemDto>();

                // Build menu tree
                foreach (var menu in allMenus.Where(m => m.ParentMenuId == null))
                {
                    var menuItem = await BuildMenuItemAsync(menu, userRoleIds);
                    if (menuItem != null)
                    {
                        userMenus.Add(menuItem);
                    }
                }

                var orderedMenus = userMenus.OrderBy(m => m.SortOrder).ToList();

                return Result<IEnumerable<MenuItemDto>>.Success(
                    orderedMenus,
                    $"Retrieved {orderedMenus.Count} accessible menus"
                );
            }
            catch (Exception ex)
            {
                // ✅ Use new Result.Failure with exception message
                return Result<IEnumerable<MenuItemDto>>.Failure(
                    "Failed to retrieve user menus",
                    ex.Message
                );
            }
        }

        public async Task<Result<MenuPermissions>> GetUserMenuPermissionsAsync(Guid userId, Guid menuId)
        {
            try
            {
                // ✅ Direct call without checking Status/Data
                var userRoles = await _userService.GetUserRolesAsync(userId);

                if (userRoles == null || !userRoles.Any())
                {
                    return Result<MenuPermissions>.Failure(
                        "No roles found",
                        "User has no active role assignments"
                    );
                }

                var userRoleIds = userRoles.Select(r => r.Id).ToList();

                var permissions = await GetMenuPermissionsAsync(menuId, userRoleIds);

                // Check if user has any permissions
                if (!permissions.CanView && !permissions.CanAdd && !permissions.CanEdit &&
                    !permissions.CanDelete && !permissions.CanExport && !permissions.CanPrint &&
                    !permissions.CanApprove && !permissions.CanReject)
                {
                    return Result<MenuPermissions>.Failure(
                        "No permissions found",
                        "User does not have access to this menu"
                    );
                }

                return Result<MenuPermissions>.Success(
                    permissions,
                    "Menu permissions retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return Result<MenuPermissions>.Failure(
                    "Failed to retrieve menu permissions",
                    ex.Message
                );
            }
        }

        public async Task<Result<bool>> HasPermissionAsync(Guid userId, string permissionName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionName))
                {
                    return Result<bool>.Failure(
                        "Invalid permission name",
                        "Permission name cannot be null or empty"
                    );
                }

                var userRoles = await _userService.GetUserRolesAsync(userId);

                if (userRoles == null || !userRoles.Any())
                {
                    return Result<bool>.Success(false, "User has no active roles");
                }

                // Check if user has active roles
                var hasActiveRole = userRoles.Any(role => role.IsActive);

                return Result<bool>.Success(
                    hasActiveRole,
                    hasActiveRole
                        ? $"User has active role with permission '{permissionName}'"
                        : "User does not have any active roles"
                );
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure("Failed to check permission", ex.Message);
            }
        }

        public async Task<Result<bool>> HasMenuAccessAsync(Guid userId, Guid menuId)
        {
            try
            {
                var userRoles = await _userService.GetUserRolesAsync(userId);

                if (userRoles == null || !userRoles.Any())
                {
                    return Result<bool>.Success(false, "User has no active roles");
                }

                var userRoleIds = userRoles.Select(r => r.Id).ToList();
                var hasAccess = await CheckMenuAccessAsync(menuId, userRoleIds);

                return Result<bool>.Success(
                    hasAccess,
                    hasAccess
                        ? "User has access to this menu"
                        : "User does not have access to this menu"
                );
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure("Failed to check menu access", ex.Message);
            }
        }

        public async Task<Result> AssignMenuPermissionsToRoleAsync(
            Guid roleId,
            Dictionary<Guid, MenuPermissions> menuPermissions)
        {
            try
            {
                if (menuPermissions == null || !menuPermissions.Any())
                {
                    return Result.Failure(
                        "Invalid input",
                        "Menu permissions dictionary cannot be null or empty"
                    );
                }

                var assignedCount = 0;
                var updatedCount = 0;
                var errors = new List<string>();

                foreach (var kvp in menuPermissions)
                {
                    var menuId = kvp.Key;
                    var permissions = kvp.Value;

                    try
                    {
                        var existingPermission = await _roleMenuPermissionRepository
                            .GetByRoleAndMenuAsync(roleId, menuId);

                        if (existingPermission != null)
                        {
                            existingPermission.SetPermissions(permissions);
                            await _roleMenuPermissionRepository.UpdateAsync(existingPermission);
                            updatedCount++;
                        }
                        else
                        {
                            var newPermission = new RoleMenuPermission(roleId, menuId, permissions);
                            await _roleMenuPermissionRepository.CreateAsync(newPermission);
                            assignedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to assign permission for menu {menuId}: {ex.Message}");
                    }
                }

                if (errors.Any())
                {
                    return Result.Failure("Partial success", errors);
                }

                return Result.Success(
                    $"Successfully assigned/updated permissions: {assignedCount} new, {updatedCount} updated"
                );
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to assign menu permissions", ex.Message);
            }
        }

        public async Task<Result> RevokeMenuPermissionsFromRoleAsync(
            Guid roleId,
            IEnumerable<Guid> menuIds)
        {
            try
            {
                if (menuIds == null || !menuIds.Any())
                {
                    return Result.Failure(
                        "Invalid input",
                        "Menu IDs collection cannot be null or empty"
                    );
                }

                var revokedCount = 0;
                var errors = new List<string>();

                foreach (var menuId in menuIds)
                {
                    try
                    {
                        await _roleMenuPermissionRepository.DeleteAsync(roleId, menuId);
                        revokedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to revoke permission for menu {menuId}: {ex.Message}");
                    }
                }

                if (errors.Any())
                {
                    return Result.Failure("Partial success", errors);
                }

                return Result.Success($"Successfully revoked {revokedCount} menu permissions");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to revoke menu permissions", ex.Message);
            }
        }

        public async Task<Result<IEnumerable<MenuItemDto>>> GetRoleMenusAsync(Guid roleId)
        {
            try
            {
                var allMenus = await _menuRepository.GetMenuHierarchyAsync();
                var roleMenus = new List<MenuItemDto>();

                foreach (var menu in allMenus)
                {
                    var permission = await _roleMenuPermissionRepository.GetByRoleAndMenuAsync(roleId, menu.Id);
                    if (permission != null && permission.CanView)
                    {
                        roleMenus.Add(new MenuItemDto
                        {
                            Id = menu.Id,
                            Name = menu.Name,
                            DisplayName = menu.DisplayName,
                            Icon = menu.Icon,
                            Route = menu.Route,
                            ParentId = menu.ParentMenuId,
                            SortOrder = menu.SortOrder,
                            Permissions = permission.GetPermissions()
                        });
                    }
                }

                return Result<IEnumerable<MenuItemDto>>.Success(
                    roleMenus.OrderBy(m => m.SortOrder),
                    $"Retrieved {roleMenus.Count} menus for role"
                );
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<MenuItemDto>>.Failure(
                    "Failed to retrieve role menus",
                    ex.Message
                );
            }
        }

        public async Task<Result<Dictionary<Guid, MenuPermissions>>> GetRolePermissionMatrixAsync(Guid roleId)
        {
            try
            {
                var allMenus = await _menuRepository.GetMenuHierarchyAsync();
                var matrix = new Dictionary<Guid, MenuPermissions>();

                foreach (var menu in allMenus)
                {
                    var permission = await _roleMenuPermissionRepository.GetByRoleAndMenuAsync(roleId, menu.Id);
                    if (permission != null)
                    {
                        matrix[menu.Id] = permission.GetPermissions();
                    }
                }

                return Result<Dictionary<Guid, MenuPermissions>>.Success(
                    matrix,
                    $"Retrieved permission matrix with {matrix.Count} entries"
                );
            }
            catch (Exception ex)
            {
                return Result<Dictionary<Guid, MenuPermissions>>.Failure(
                    "Failed to retrieve permission matrix",
                    ex.Message
                );
            }
        }

        public async Task<Result> UpdateMenuPermissionAsync(
            Guid roleId,
            Guid menuId,
            MenuPermissions permissions)
        {
            try
            {
                var existingPermission = await _roleMenuPermissionRepository.GetByRoleAndMenuAsync(roleId, menuId);

                if (existingPermission == null)
                {
                    return Result.Failure(
                        "Permission not found",
                        "No permission entry exists for this role-menu combination"
                    );
                }

                existingPermission.SetPermissions(permissions);
                await _roleMenuPermissionRepository.UpdateAsync(existingPermission);

                return Result.Success("Menu permission updated successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to update menu permission", ex.Message);
            }
        }

        public async Task<Result> BulkAssignMenuPermissionsAsync(
            Guid roleId,
            List<MenuPermissionAssignmentDto> assignments)
        {
            try
            {
                if (assignments == null || !assignments.Any())
                {
                    return Result.Failure(
                        "Invalid input",
                        "Assignments list cannot be null or empty"
                    );
                }

                var menuPermissions = assignments.ToDictionary(a => a.MenuId, a => a.Permissions);
                return await AssignMenuPermissionsToRoleAsync(roleId, menuPermissions);
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to bulk assign menu permissions", ex.Message);
            }
        }

        public async Task<Result<bool>> RoleHasMenuPermissionAsync(
            Guid roleId,
            Guid menuId,
            string permissionType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionType))
                {
                    return Result<bool>.Failure(
                        "Invalid permission type",
                        "Permission type cannot be null or empty"
                    );
                }

                var permission = await _roleMenuPermissionRepository.GetByRoleAndMenuAsync(roleId, menuId);

                if (permission == null)
                {
                    return Result<bool>.Success(false, "No permission entry found");
                }

                var permissions = permission.GetPermissions();
                var hasPermission = permissionType.ToLower() switch
                {
                    "canview" => permissions.CanView,
                    "canadd" => permissions.CanAdd,
                    "canedit" => permissions.CanEdit,
                    "candelete" => permissions.CanDelete,
                    "canexport" => permissions.CanExport,
                    "canprint" => permissions.CanPrint,
                    "canapprove" => permissions.CanApprove,
                    "canreject" => permissions.CanReject,
                    _ => false
                };

                return Result<bool>.Success(
                    hasPermission,
                    $"Permission check completed for {permissionType}"
                );
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure("Failed to check role menu permission", ex.Message);
            }
        }

        #region Private Helper Methods

        private async Task<MenuItemDto> BuildMenuItemAsync(Menu menu, List<Guid> userRoleIds)
        {
            var hasAccess = await CheckMenuAccessAsync(menu.Id, userRoleIds);
            if (!hasAccess || !menu.IsActive || !menu.IsVisible)
                return null;

            var menuItem = new MenuItemDto
            {
                Id = menu.Id,
                Name = menu.Name,
                DisplayName = menu.DisplayName,
                ParentId = menu.ParentMenuId,  // ✅ Nullable, no casting
                Icon = menu.Icon,
                Route = menu.Route,
                Component = menu.Component,
                Type = menu.Type.ToString(),
                SortOrder = menu.SortOrder,
                Children = new List<MenuItemDto>()
            };

            // Get permissions for this menu
            var permissions = await GetMenuPermissionsAsync(menu.Id, userRoleIds);
            menuItem.Permissions = permissions;

            // Recursively build child menus
            if (menu.SubMenus != null && menu.SubMenus.Any())
            {
                var childMenus = menu.SubMenus.Where(sm => sm.IsActive && sm.IsVisible);
                foreach (var childMenu in childMenus)
                {
                    var childMenuItem = await BuildMenuItemAsync(childMenu, userRoleIds);
                    if (childMenuItem != null)
                    {
                        menuItem.Children.Add(childMenuItem);
                    }
                }
            }

            return menuItem;
        }

        private async Task<MenuPermissions> GetMenuPermissionsAsync(Guid menuId, List<Guid> roleIds)
        {
            var permissions = new MenuPermissions
            {
                CanView = false,
                CanAdd = false,
                CanEdit = false,
                CanDelete = false,
                CanExport = false,
                CanPrint = false,
                CanApprove = false,
                CanReject = false
            };

            foreach (var roleId in roleIds)
            {
                var roleMenuPermission = await _roleMenuPermissionRepository.GetByRoleAndMenuAsync(roleId, menuId);
                if (roleMenuPermission != null)
                {
                    var rolePermissions = roleMenuPermission.GetPermissions();

                    // Combine permissions (OR operation)
                    permissions.CanView |= rolePermissions.CanView;
                    permissions.CanAdd |= rolePermissions.CanAdd;
                    permissions.CanEdit |= rolePermissions.CanEdit;
                    permissions.CanDelete |= rolePermissions.CanDelete;
                    permissions.CanExport |= rolePermissions.CanExport;
                    permissions.CanPrint |= rolePermissions.CanPrint;
                    permissions.CanApprove |= rolePermissions.CanApprove;
                    permissions.CanReject |= rolePermissions.CanReject;
                }
            }

            return permissions;
        }

        private async Task<bool> CheckMenuAccessAsync(Guid menuId, List<Guid> roleIds)
        {
            foreach (var roleId in roleIds)
            {
                var roleMenuPermission = await _roleMenuPermissionRepository.GetByRoleAndMenuAsync(roleId, menuId);
                if (roleMenuPermission?.CanView == true)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}