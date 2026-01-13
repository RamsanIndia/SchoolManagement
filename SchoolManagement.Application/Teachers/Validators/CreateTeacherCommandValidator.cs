using FluentValidation;
using SchoolManagement.Application.Teachers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Validators
{
    public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
    {
        public CreateTeacherCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be 10-15 digits");

            RuleFor(x => x.EmployeeCode)
                .NotEmpty().WithMessage("Employee ID is required")
                .MaximumLength(20).WithMessage("Employee ID cannot exceed 20 characters");

            RuleFor(x => x.DateOfJoining)
                .NotEmpty().WithMessage("Date of joining is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date of joining cannot be in the future");

            RuleFor(x => x.Qualification)
                .NotEmpty().WithMessage("Qualification is required")
                .MaximumLength(200).WithMessage("Qualification cannot exceed 200 characters");

            RuleFor(x => x.PriorExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Experience cannot be negative")
                .LessThanOrEqualTo(50).WithMessage("Experience cannot exceed 50 years");

            // Address validation
            RuleFor(x => x.Address.Street)
                .NotEmpty().WithMessage("Street is required");

            RuleFor(x => x.Address.City)
                .NotEmpty().WithMessage("City is required");

            RuleFor(x => x.Address.State)
                .NotEmpty().WithMessage("State is required");

            RuleFor(x => x.Address.ZipCode)
                .NotEmpty().WithMessage("Postal code is required");

            RuleFor(x => x.Address.Country)
                .NotEmpty().WithMessage("Country is required");

            RuleFor(x => x.DepartmentId)
                .NotEqual(Guid.Empty).When(x => x.DepartmentId.HasValue)
                .WithMessage("Invalid department ID");
        }
    }
}
