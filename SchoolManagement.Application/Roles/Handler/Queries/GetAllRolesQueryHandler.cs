using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Roles.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Roles.Handler.Queries
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _unitOfWork.RoleRepository.GetAllAsync();

            return roles.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                Level = role.Level
                //UserCount = role.UserCount
            });
        }
    }
}
