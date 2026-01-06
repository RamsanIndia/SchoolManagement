using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolManagement.API.Authorization
{
    public class MenuPermissionHandler : AuthorizationHandler<MenuPermissionAttribute>
    {
        private readonly IMenuPermissionService _menuPermissionService;
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<MenuPermissionHandler> _logger;

        public MenuPermissionHandler(
            IMenuPermissionService menuPermissionService,
            IMenuRepository menuRepository,
            ILogger<MenuPermissionHandler> logger)
        {
            _menuPermissionService = menuPermissionService;
            _menuRepository = menuRepository;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MenuPermissionAttribute requirement)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? context.User.FindFirst("sub")?.Value
                               ?? context.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid or missing user identifier in claims");
                    context.Fail();
                    return;
                }

                // Find menu by name
                var menus = await _menuRepository.GetAllAsync();
                var menu = menus.FirstOrDefault(m =>
                    m.Name.Equals(requirement.MenuName, StringComparison.OrdinalIgnoreCase));

                if (menu == null)
                {
                    _logger.LogWarning("Menu not found: {MenuName}", requirement.MenuName);
                    context.Fail();
                    return;
                }

                // ✅ Check if user has access to the menu
                var hasAccessResult = await _menuPermissionService.HasMenuAccessAsync(userId, menu.Id);

                if (!hasAccessResult.Status)
                {
                    _logger.LogInformation(
                        "Failed to check menu access for user {UserId} and menu {MenuName}: {Message}",
                        userId,
                        requirement.MenuName,
                        hasAccessResult.Message
                    );
                    context.Fail();
                    return;
                }

                // ✅ Extract boolean value from Result<bool>
                if (!hasAccessResult.Data)
                {
                    _logger.LogInformation(
                        "User {UserId} does not have access to menu {MenuName}",
                        userId,
                        requirement.MenuName
                    );
                    context.Fail();
                    return;
                }

                // Check specific permissions if required
                if (requirement.RequiredPermissions?.Length > 0)
                {
                    // ✅ Get permissions result
                    var userPermissionsResult = await _menuPermissionService
                        .GetUserMenuPermissionsAsync(userId, menu.Id);

                    if (!userPermissionsResult.Status)
                    {
                        _logger.LogInformation(
                            "Failed to get permissions for user {UserId} and menu {MenuName}: {Message}",
                            userId,
                            requirement.MenuName,
                            userPermissionsResult.Message
                        );
                        context.Fail();
                        return;
                    }

                    // ✅ Extract MenuPermissions from Result<MenuPermissions>
                    var userPermissions = userPermissionsResult.Data;

                    if (userPermissions == null)
                    {
                        _logger.LogWarning(
                            "Permissions data is null for user {UserId} and menu {MenuName}",
                            userId,
                            requirement.MenuName
                        );
                        context.Fail();
                        return;
                    }

                    // Check each required permission
                    foreach (var requiredPermission in requirement.RequiredPermissions)
                    {
                        var hasPermission = requiredPermission.ToLower() switch
                        {
                            "view" => userPermissions.CanView,
                            "add" => userPermissions.CanAdd,
                            "edit" => userPermissions.CanEdit,
                            "delete" => userPermissions.CanDelete,
                            "export" => userPermissions.CanExport,
                            "print" => userPermissions.CanPrint,
                            "approve" => userPermissions.CanApprove,
                            "reject" => userPermissions.CanReject,
                            _ => false
                        };

                        if (!hasPermission)
                        {
                            _logger.LogInformation(
                                "User {UserId} does not have {Permission} permission for menu {MenuName}",
                                userId,
                                requiredPermission,
                                requirement.MenuName
                            );
                            context.Fail();
                            return;
                        }
                    }
                }

                _logger.LogDebug(
                    "User {UserId} authorized for menu {MenuName} with permissions: {Permissions}",
                    userId,
                    requirement.MenuName,
                    string.Join(", ", requirement.RequiredPermissions ?? Array.Empty<string>())
                );

                context.Succeed(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in menu permission handler for menu {MenuName}",
                    requirement.MenuName
                );
                context.Fail();
            }
        }
    }
}
