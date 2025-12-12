using FluentValidation;
using SchoolManagement.Application.UserRoles.Commands;
using System;

namespace SchoolManagement.Application.UserRoles.Validators
{
    public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
    {
        public UpdateUserRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Role ID cannot be empty.");

            // For active roles with expiration
            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future for active roles.")
                .LessThan(DateTime.UtcNow.AddYears(10)).WithMessage("Expiration date cannot be more than 10 years in the future.")
                .When(x => x.IsActive && x.ExpiresAt.HasValue);
        }
    }
}