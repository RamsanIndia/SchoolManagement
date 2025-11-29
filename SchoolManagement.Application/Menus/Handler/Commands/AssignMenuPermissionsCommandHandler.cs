using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Menus.Commands;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Menus.Handler.Commands
{
    public class AssignMenuPermissionsCommandHandler : IRequestHandler<AssignMenuPermissionsCommand, Result>
    {
        private readonly IMenuPermissionService _menuPermissionService;

        public AssignMenuPermissionsCommandHandler(IMenuPermissionService menuPermissionService)
        {
            _menuPermissionService = menuPermissionService;
        }

        public async Task<Result> Handle(AssignMenuPermissionsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var menuPermissions = request.MenuPermissions.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new MenuPermissions
                    {
                        CanView = kvp.Value.CanView,
                        CanAdd = kvp.Value.CanAdd,
                        CanEdit = kvp.Value.CanEdit,
                        CanDelete = kvp.Value.CanDelete,
                        CanExport = kvp.Value.CanExport,
                        CanPrint = kvp.Value.CanPrint,
                        CanApprove = kvp.Value.CanApprove,
                        CanReject = kvp.Value.CanReject
                    });

                await _menuPermissionService.AssignMenuPermissionsToRoleAsync(request.RoleId, menuPermissions);

                return Result.Success("Menu permissions assigned successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error assigning permissions: {ex.Message}");
            }
        }
    }
}
