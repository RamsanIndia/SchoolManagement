using FluentValidation;
using SchoolManagement.Application.Sections.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Validators
{
    public class GetSectionsQueryValidator : AbstractValidator<GetSectionsQuery>
    {
        public GetSectionsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.ClassId)
                .NotEmpty()
                .When(x => x.ClassId.HasValue)
                .WithMessage("Class ID must be a valid GUID");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
                .WithMessage("Search term cannot exceed 100 characters");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                    new[] { "name", "capacity", "createdat", "roomnumber" }.Contains(sortBy.ToLower()))
                .WithMessage("Invalid sort field. Allowed: name, capacity, createdat, roomnumber");

            RuleFor(x => x.SortDirection)
                .Must(dir => string.IsNullOrEmpty(dir) ||
                    new[] { "asc", "desc" }.Contains(dir.ToLower()))
                .WithMessage("Sort direction must be 'asc' or 'desc'");
        }
    }
}
