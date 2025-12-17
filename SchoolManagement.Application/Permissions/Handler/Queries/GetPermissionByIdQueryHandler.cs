using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Permissions.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Permissions.Handler.Queries
{
    public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPermissionByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id, cancellationToken);

            if (permission == null)
                return Result<PermissionDto>.Failure("Permission not found.");

            var dto = new PermissionDto
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

            return Result<PermissionDto>.Success(dto);
        }
    }
}
