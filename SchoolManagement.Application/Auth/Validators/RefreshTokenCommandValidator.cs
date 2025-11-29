using FluentValidation;
using SchoolManagement.Application.Auth.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.")
                .NotNull().WithMessage("Refresh token cannot be null.")
                .MinimumLength(20).WithMessage("Invalid refresh token format.")
                .MaximumLength(500).WithMessage("Refresh token is too long.");
        }
    }
}
