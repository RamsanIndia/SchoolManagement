using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IMenuPermissionService
    {
        /// <summary>
        /// Get all accessible menus for a user with their permissions
        /// </summary>
        Task<Result<IEnumerable<MenuItemDto>>> GetUserMenusAsync(Guid userId);

        /// <summary>
        /// Get user's permissions for a specific menu
        /// </summary>
        Task<Result<MenuPermissions>> GetUserMenuPermissionsAsync(Guid userId, Guid menuId);

        /// <summary>
        /// Check if user has a specific permission
        /// </summary>
        Task<Result<bool>> HasPermissionAsync(Guid userId, string permissionName);

        /// <summary>
        /// Check if user has access to view a specific menu
        /// </summary>
        Task<Result<bool>> HasMenuAccessAsync(Guid userId, Guid menuId);

        /// <summary>
        /// Assign menu permissions to a role
        /// </summary>
        Task<Result> AssignMenuPermissionsToRoleAsync(Guid roleId, Dictionary<Guid, MenuPermissions> menuPermissions);

        /// <summary>
        /// Revoke menu permissions from a role
        /// </summary>
        Task<Result> RevokeMenuPermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> menuIds);

        /// <summary>
        /// Get all menus accessible by a specific role
        /// </summary>
        Task<Result<IEnumerable<MenuItemDto>>> GetRoleMenusAsync(Guid roleId);

        /// <summary>
        /// Get permission matrix for a role (all menus with their permissions)
        /// </summary>
        Task<Result<Dictionary<Guid, MenuPermissions>>> GetRolePermissionMatrixAsync(Guid roleId);

        /// <summary>
        /// Update permissions for a specific menu-role combination
        /// </summary>
        Task<Result> UpdateMenuPermissionAsync(Guid roleId, Guid menuId, MenuPermissions permissions);

        /// <summary>
        /// Bulk assign permissions to multiple menus for a role
        /// </summary>
        Task<Result> BulkAssignMenuPermissionsAsync(Guid roleId, List<MenuPermissionAssignmentDto> assignments);

        /// <summary>
        /// Check if a role has specific permission on a menu
        /// </summary>
        Task<Result<bool>> RoleHasMenuPermissionAsync(Guid roleId, Guid menuId, string permissionType);
    }

    /// <summary>
    /// DTO for bulk menu permission assignment
    /// </summary>
    public class MenuPermissionAssignmentDto
    {
        public Guid MenuId { get; set; }
        public MenuPermissions Permissions { get; set; }
    }
}
