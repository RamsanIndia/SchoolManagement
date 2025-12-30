using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Teachers.Queries
{
    public class GetAllTeachersQuery : IRequest<Result<PagedResult<TeacherDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";

        // Parameterless constructor for model binding
        public GetAllTeachersQuery()
        {
        }

        // Constructor with validation
        public GetAllTeachersQuery(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            bool? isActive = null,
            Guid? departmentId = null,
            string? sortBy = null,
            string? sortDirection = "asc")
        {
            // Validate and normalize page number
            PageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Validate and normalize page size (min: 1, max: 100)
            PageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

            // Trim search term
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

            // Set filters
            IsActive = isActive;
            DepartmentId = departmentId;

            // Whitelist and validate sort field
            var allowedSortFields = new[] { "firstname", "lastname", "employeeid", "dateofjoining", "experience" };
            SortBy = allowedSortFields.Contains(sortBy?.ToLower()) ? sortBy.ToLower() : "lastname";

            // Validate sort direction
            SortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";
        }
    }
}
