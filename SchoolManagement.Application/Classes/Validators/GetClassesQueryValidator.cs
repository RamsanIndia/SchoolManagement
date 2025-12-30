using FluentValidation;
using SchoolManagement.Application.Classes.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Validators
{
    public class GetClassesQueryValidator : AbstractValidator<GetClassesQuery>
    {
        public GetClassesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.AcademicYearId)
                .NotEmpty()
                .When(x => x.AcademicYearId.HasValue)
                .WithMessage("Academic year ID must be a valid GUID");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
                .WithMessage("Search term cannot exceed 100 characters");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                    new[] { "name", "code", "grade", "createdat" }.Contains(sortBy.ToLower()))
                .WithMessage("Invalid sort field. Allowed: name, code, grade, createdat");

            RuleFor(x => x.SortDirection)
                .Must(dir => string.IsNullOrEmpty(dir) ||
                    new[] { "asc", "desc" }.Contains(dir.ToLower()))
                .WithMessage("Sort direction must be 'asc' or 'desc'");
        }
    }
}
