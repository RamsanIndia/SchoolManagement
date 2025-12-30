using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Sections.Queries
{
    public class GetSectionsQuery : IRequest<Result<PagedResult<SectionListDto>>>
    {
        public Guid? ClassId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";

        // Parameterless constructor for model binding
        public GetSectionsQuery()
        {
        }

        // Constructor with validation
        public GetSectionsQuery(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? classId = null,
            bool? isActive = null,
            string? searchTerm = null,
            string? sortBy = null,
            string? sortDirection = "asc")
        {
            // Validate and normalize page number
            PageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Validate and normalize page size (min: 1, max: 100)
            PageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

            // Set filters
            ClassId = classId;
            IsActive = isActive;

            // Trim search term
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

            // Whitelist and validate sort field
            var allowedSortFields = new[] { "name", "capacity", "createdat", "roomnumber" };
            SortBy = allowedSortFields.Contains(sortBy?.ToLower()) ? sortBy.ToLower() : "name";

            // Validate sort direction
            SortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";
        }
    }
}
