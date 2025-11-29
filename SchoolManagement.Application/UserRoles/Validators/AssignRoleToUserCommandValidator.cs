using FluentValidation;
using SchoolManagement.Application.UserRoles.Commands;
using System;

namespace SchoolManagement.Application.UserRoles.Validators
{
    public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
    {
        public AssignRoleToUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Role ID cannot be empty.");

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.")
                .When(x => x.ExpiresAt.HasValue);
        }
    }
}