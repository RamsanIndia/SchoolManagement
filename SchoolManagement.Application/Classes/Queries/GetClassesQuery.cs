using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Classes.Queries
{
    public class GetClassesQuery : IRequest<Result<PagedResult<ClassDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public Guid? AcademicYearId { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";

        // Parameterless constructor for model binding
        public GetClassesQuery()
        {
        }

        // Constructor with validation
        public GetClassesQuery(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            bool? isActive = null,
            Guid? academicYearId = null,
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
            AcademicYearId = academicYearId;

            // Whitelist and validate sort field
            var allowedSortFields = new[] { "name", "code", "grade", "createdat" };
            SortBy = allowedSortFields.Contains(sortBy?.ToLower()) ? sortBy.ToLower() : "name";

            // Validate sort direction
            SortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";
        }
    }
}
