using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Permissions.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using PermissionEntity = SchoolManagement.Domain.Entities.Permission;

namespace SchoolManagement.Application.Permissions.Handler.Commands
{
    public class CreatePermissionCommandHandler
        : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePermissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
        {
            // Check if permission already exists (Module + Action + Resource)
            var exists = await _unitOfWork.Permissions.ExistsAsync(
                p => p.Module == request.Module &&
                     p.Action == request.Action &&
                     p.Resource == request.Resource,
                cancellationToken
            );

            if (exists)
            {
                return Result<PermissionDto>.Failure(
                    "Duplicate permission",
                    "A permission with the same Module, Action, and Resource already exists."
                );
            }

            // Create new permission entity
            var permission = new PermissionEntity(
                request.Name,
                request.DisplayName,
                request.Module,
                request.Action,
                request.Resource,
                request.Description
            );

            await _unitOfWork.Permissions.AddAsync(permission, cancellationToken);
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

            return Result<PermissionDto>.Success(permissionDto, "Permission created successfully");
        }
    }
}