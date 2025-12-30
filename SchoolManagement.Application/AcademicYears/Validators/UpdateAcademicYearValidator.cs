using FluentValidation;
using SchoolManagement.Application.AcademicYears.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Validators
{
    public class UpdateAcademicYearValidator : AbstractValidator<UpdateAcademicYearCommand>
    {
        public UpdateAcademicYearValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Academic year ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Academic year name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .Must((command, endDate) => endDate > command.StartDate)
                .WithMessage("End date must be after start date");
        }
    }
}
