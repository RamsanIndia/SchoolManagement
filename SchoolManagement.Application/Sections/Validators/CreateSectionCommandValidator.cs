using FluentValidation;
using SchoolManagement.Application.Sections.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Validators
{
    public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
    {
        public CreateSectionCommandValidator()
        {
            RuleFor(x => x.ClassId)
                .NotEmpty()
                .WithMessage("Class ID is required.");

            RuleFor(x => x.SectionName)
                .NotEmpty()
                .WithMessage("Section name is required.")
                .MaximumLength(50)
                .WithMessage("Section name cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9\s\-]+$")
                .WithMessage("Section name can only contain letters, numbers, spaces, and hyphens.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Capacity must be greater than zero.")
                .LessThanOrEqualTo(100)
                .WithMessage("Capacity cannot exceed 100 students.");

            RuleFor(x => x.RoomNumber)
                .NotEmpty()
                .WithMessage("Room number is required.")
                .MaximumLength(20)
                .WithMessage("Room number cannot exceed 20 characters.");

            
        }
    }
}
