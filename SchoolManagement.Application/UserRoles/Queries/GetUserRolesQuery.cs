using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.UserRoles.Queries
{
    public class GetUserRolesQuery : IRequest<Result<IEnumerable<UserRoleDto>>>
    {
        public Guid UserId { get; set; }
    }
}
