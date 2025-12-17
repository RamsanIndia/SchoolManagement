using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Roles.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Roles.Handler.Queries
{
    public class GetRoleMenuPermissionsQueryHandler : IRequestHandler<GetRoleMenuPermissionsQuery, Result<IEnumerable<RoleMenuPermissionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRoleMenuPermissionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<RoleMenuPermissionDto>>> Handle(GetRoleMenuPermissionsQuery request, CancellationToken cancellationToken)
        {
            // Get role entity
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId,cancellationToken);
            if (role == null)
                return Result<IEnumerable<RoleMenuPermissionDto>>.Failure("Role not found");

            // Get menu permissions for the role
            var roleMenuPermissions = await _unitOfWork.RoleMenuPermissionRepository.GetByRoleAsync(request.RoleId);

            // Map to DTO
            var result = roleMenuPermissions.Select(p => new RoleMenuPermissionDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                MenuId = p.MenuId,
                MenuName = p.Menu?.Name ?? string.Empty,
                Permissions = p.GetPermissions()
            });

            return Result<IEnumerable<RoleMenuPermissionDto>>.Success(result);
        }
    }
}
