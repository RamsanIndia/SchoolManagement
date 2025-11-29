using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.UserRoles.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.UserRoles.Handlers
{
    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<IEnumerable<UserRoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserRolesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<UserRoleDto>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = request.UserId == Guid.Empty
                    ? await _unitOfWork.UserRoleRepository.GetAllWithUserAndRoleAsync(cancellationToken)
                    : await _unitOfWork.UserRoleRepository.FindWithUserAndRoleAsync(
                        ur => ur.UserId == request.UserId,
                        cancellationToken);

                if (userRoles == null || !userRoles.Any())
                    return Result<IEnumerable<UserRoleDto>>.Success(Enumerable.Empty<UserRoleDto>());

                var now = DateTime.UtcNow;

                var result = userRoles.Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    Username = ur.User?.Username ?? string.Empty,
                    FullName = ur.User != null
                        ? $"{ur.User.FirstName} {ur.User.LastName}".Trim()
                        : string.Empty,
                    RoleId = ur.RoleId,
                    RoleName = ur.Role?.Name ?? string.Empty,
                    AssignedAt = ur.AssignedAt,
                    ExpiresAt = ur.ExpiresAt,
                    IsActive = ur.IsActive,
                    IsExpired = ur.ExpiresAt.HasValue && ur.ExpiresAt.Value < now
                });

                return Result<IEnumerable<UserRoleDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<UserRoleDto>>.Failure($"Failed to fetch user roles: {ex.Message}");
            }
        }

    }
}
