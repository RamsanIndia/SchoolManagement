using FluentValidation;
using SchoolManagement.Application.Classes.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Validators
{
    public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
    {
        public CreateClassCommandValidator()
        {
            RuleFor(x => x.ClassName)
                .NotEmpty().WithMessage("Class name is required")
                .MaximumLength(100).WithMessage("Class name cannot exceed 100 characters");

            RuleFor(x => x.ClassCode)
                .NotEmpty().WithMessage("Class code is required")
                .MaximumLength(20).WithMessage("Class code cannot exceed 20 characters");

            RuleFor(x => x.Grade)
                .GreaterThan(0).WithMessage("Grade must be greater than 0")
                .LessThanOrEqualTo(12).WithMessage("Grade cannot exceed 12");

            RuleFor(x => x.AcademicYearId)
                .NotEmpty().WithMessage("Academic year is required");
        }
    }
}
