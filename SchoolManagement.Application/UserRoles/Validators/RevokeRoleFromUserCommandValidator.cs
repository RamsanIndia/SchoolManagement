using FluentValidation;
using SchoolManagement.Application.UserRoles.Commands;
using System;

namespace SchoolManagement.Application.UserRoles.Validators
{
    public class RevokeRoleFromUserCommandValidator : AbstractValidator<RevokeRoleFromUserCommand>
    {
        public RevokeRoleFromUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Role ID cannot be empty.");
        }
    }
}