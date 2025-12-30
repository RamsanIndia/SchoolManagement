using FluentValidation;
using SchoolManagement.Application.SectionSubjects.Queries;

namespace SchoolManagement.Application.SectionSubjects.Validators
{
    public class GetSectionSubjectsQueryValidator : AbstractValidator<GetSectionSubjectsQuery>
    {
        public GetSectionSubjectsQueryValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty()
                .WithMessage("Section ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
                .WithMessage("Search term cannot exceed 100 characters");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                    new[] { "subjectname", "teachername", "weeklyperiods" }.Contains(sortBy.ToLower()))
                .WithMessage("Invalid sort field. Allowed values: subjectName, teacherName, weeklyPeriods");

            RuleFor(x => x.SortDirection)
                .Must(dir => string.IsNullOrEmpty(dir) ||
                    new[] { "asc", "desc" }.Contains(dir.ToLower()))
                .WithMessage("Sort direction must be 'asc' or 'desc'");
        }
    }
}
