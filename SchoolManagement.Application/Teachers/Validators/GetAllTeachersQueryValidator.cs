using FluentValidation;
using SchoolManagement.Application.Teachers.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Validators
{
    public class GetAllTeachersQueryValidator : AbstractValidator<GetAllTeachersQuery>
    {
        public GetAllTeachersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .When(x => x.DepartmentId.HasValue)
                .WithMessage("Department ID must be a valid GUID");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
                .WithMessage("Search term cannot exceed 100 characters");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                    new[] { "firstname", "lastname", "employeeid", "dateofjoining", "experience" }.Contains(sortBy.ToLower()))
                .WithMessage("Invalid sort field. Allowed: firstname, lastname, employeeid, dateofjoining, experience");

            RuleFor(x => x.SortDirection)
                .Must(dir => string.IsNullOrEmpty(dir) ||
                    new[] { "asc", "desc" }.Contains(dir.ToLower()))
                .WithMessage("Sort direction must be 'asc' or 'desc'");
        }
    }
}
