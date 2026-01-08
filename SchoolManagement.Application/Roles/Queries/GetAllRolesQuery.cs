using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<Result<PagedResult<RoleDto>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSystemRole { get; set; }
        public int? Level { get; set; }
        public string? SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
