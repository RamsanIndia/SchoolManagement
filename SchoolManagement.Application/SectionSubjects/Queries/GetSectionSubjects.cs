using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.SectionSubjects.Queries
{
    public class GetSectionSubjectsQuery : IRequest<Result<PagedResult<SectionSubjectDto>>>
    {
        public Guid SectionId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsMandatory { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";

        // Parameterless constructor for model binding
        public GetSectionSubjectsQuery()
        {
        }

        // Constructor with validation
        public GetSectionSubjectsQuery(
            Guid sectionId,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            bool? isMandatory = null,
            string? sortBy = null,
            string? sortDirection = "asc")
        {
            SectionId = sectionId;

            // Validate and normalize page number
            PageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Validate and normalize page size (min: 1, max: 100)
            PageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

            // Trim search term
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

            // Set mandatory filter
            IsMandatory = isMandatory;

            // Whitelist and validate sort field
            var allowedSortFields = new[] { "subjectname", "teachername", "weeklyperiods" };
            SortBy = allowedSortFields.Contains(sortBy?.ToLower()) ? sortBy.ToLower() : "subjectname";

            // Validate sort direction
            SortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";
        }
    }
}
