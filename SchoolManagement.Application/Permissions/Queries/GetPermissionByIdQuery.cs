using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System;

namespace SchoolManagement.Application.Permissions.Queries
{
    public class GetPermissionByIdQuery : IRequest<Result<PermissionDto>>
    {
        public Guid Id { get; set; }
    }
}
