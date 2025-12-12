using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Roles.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Roles.Handler.Queries
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(request.Id,cancellationToken);

            if (role == null)
                return Result<RoleDto>.Failure("Role not found");

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                Level = role.Level
                // UserCount = await _unitOfWork.UserRoleRepository.CountUsersByRoleIdAsync(role.Id) // optional
            };

            return Result<RoleDto>.Success(roleDto);
        }
    }
}
