using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System.Collections.Generic;

namespace SchoolManagement.Application.Permissions.Queries
{
    public class GetPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
}
