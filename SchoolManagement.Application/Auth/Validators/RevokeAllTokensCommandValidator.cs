using FluentValidation;
using SchoolManagement.Application.Auth.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Validators
{
    public class RevokeAllTokensCommandValidator : AbstractValidator<RevokeAllTokensCommand>
    {
        public RevokeAllTokensCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.")
                .Must(BeValidGuid).WithMessage("Invalid User ID format.");
        }

        private bool BeValidGuid(Guid userId)
        {
            // Additional validation if needed
            // For example, check if it's not a special/reserved GUID
            return userId != Guid.Empty && userId != default;
        }
    }
}
