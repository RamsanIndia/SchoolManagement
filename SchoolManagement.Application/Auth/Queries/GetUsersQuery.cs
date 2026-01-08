using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Queries
{
    public class GetUsersQuery : IRequest<Result<PagedResult<UserDto>>>
    {
        public string? SearchTerm { get; set; }
        public string? UserType { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsPhoneVerified { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "firstname";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
