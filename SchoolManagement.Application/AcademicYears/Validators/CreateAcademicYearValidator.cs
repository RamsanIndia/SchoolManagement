using FluentValidation;
using SchoolManagement.Application.AcademicYears.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Validators
{
    public class CreateAcademicYearValidator : AbstractValidator<CreateAcademicYearCommand>
    {
        public CreateAcademicYearValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Academic year name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.StartYear)
                .GreaterThan(2000).WithMessage("Start year must be after 2000")
                .LessThan(2100).WithMessage("Start year must be before 2100");

            RuleFor(x => x.EndYear)
                .Must((command, endYear) => endYear == command.StartYear + 1)
                .WithMessage("End year must be one year after start year");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .Must((command, endDate) => endDate > command.StartDate)
                .WithMessage("End date must be after start date");
        }
    }
}
