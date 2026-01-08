using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System.Collections.Generic;

namespace SchoolManagement.Application.Permissions.Queries
{
    public class GetPermissionsQuery : IRequest<Result<PagedResult<PermissionDto>>>
    {
        public string? SearchTerm { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public string? Resource { get; set; }
        public bool? IsSystemPermission { get; set; }
        public string? SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
