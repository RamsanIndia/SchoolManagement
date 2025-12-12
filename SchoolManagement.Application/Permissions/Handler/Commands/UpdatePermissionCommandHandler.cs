using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Permissions.Commands;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Permissions.Handler.Commands
{
    public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePermissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            // Validate permission exists
            var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id, cancellationToken);
            if (permission == null)
                return Result<PermissionDto>.Failure("Permission not found", "The requested permission does not exist.");

            // Check system permission constraint
            if (permission.IsSystemPermission)
                return Result<PermissionDto>.Failure("Update not allowed", "System permissions cannot be modified.");

            // Check for duplicate permission
            var exists = await _unitOfWork.Permissions.ExistsAsync(
                p => p.Module == request.Module &&
                     p.Action == request.Action &&
                     p.Resource == request.Resource &&
                     p.Id != request.Id,
                cancellationToken
            );

            if (exists)
            {
                return Result<PermissionDto>.Failure(
                    "Duplicate permission",
                    "A permission with the same Module, Action, and Resource already exists."
                );
            }

            // Update using domain logic
            permission.Update(
                request.Name,
                request.DisplayName,
                request.Module,
                request.Action,
                request.Resource,
                request.Description
            );

            await _unitOfWork.Permissions.UpdateAsync(permission, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var permissionDto = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                DisplayName = permission.DisplayName,
                Module = permission.Module,
                Action = permission.Action,
                Resource = permission.Resource,
                Description = permission.Description,
                IsSystemPermission = permission.IsSystemPermission
            };

            return Result<PermissionDto>.Success(permissionDto, "Permission updated successfully");
        }
    }
}
