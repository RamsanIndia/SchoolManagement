using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Teachers.Queries
{
    public class GetTeachersByDepartmentQuery : IRequest<Result<PagedResult<TeacherDto>>>
    {
        public Guid DepartmentId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";

        // Parameterless constructor
        public GetTeachersByDepartmentQuery()
        {
        }

        // Constructor with validation
        public GetTeachersByDepartmentQuery(
            Guid departmentId,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            bool? isActive = null,
            string? sortBy = null,
            string? sortDirection = "asc")
        {
            DepartmentId = departmentId;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
            IsActive = isActive;

            var allowedSortFields = new[] { "firstname", "lastname", "employeeid", "dateofjoining", "experience" };
            SortBy = allowedSortFields.Contains(sortBy?.ToLower()) ? sortBy.ToLower() : "lastname";
            SortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";
        }
    }
}
