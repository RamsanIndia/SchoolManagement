using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Permissions.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Permissions.Handler.Commands
{
    public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletePermissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            // Validate permission exists
            var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id, cancellationToken);
            if (permission == null)
            {
                return Result.Failure("Permission not found", "The requested permission does not exist.");
            }

            // Check system permission constraint
            if (permission.IsSystemPermission)
            {
                return Result.Failure("Delete not allowed", "System permissions cannot be deleted.");
            }

            // Soft delete the permission
            await _unitOfWork.Permissions.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Permission deleted successfully");
        }
    }
}