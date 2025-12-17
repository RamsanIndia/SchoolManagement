using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Permissions.Queries
{
    public class GetPermissionByIdQuery : IRequest<Result<PermissionDto>>
    {
        public Guid Id { get; set; }
    }
}
