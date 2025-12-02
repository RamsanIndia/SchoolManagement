using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SchoolManagement.Application.Models.Result;

namespace SchoolManagement.Application.Classes.Queries
{
    public class GetClassesQuery : IRequest<Result<PagedResult<ClassDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }
}
