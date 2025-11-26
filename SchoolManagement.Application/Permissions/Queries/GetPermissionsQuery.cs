using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System.Collections.Generic;

namespace SchoolManagement.Application.Permissions.Queries
{
    public class GetPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
}
