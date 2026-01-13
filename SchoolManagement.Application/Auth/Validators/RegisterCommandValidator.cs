// Application/Auth/Validators/RegisterCommandValidator.cs
using FluentValidation;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Services;
using SchoolManagement.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Validators
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RegisterCommandValidator> _logger;
        private readonly ITenantService _tenantService;

        public RegisterCommandValidator(IUserRepository userRepository, ILogger<RegisterCommandValidator> logger, ITenantService tenantService)
        {
            _userRepository = userRepository;
            _logger = logger;

            // Username validation
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots, and hyphens")
                .MustAsync(BeUniqueUsername).WithMessage("Username is already taken");

            // Email validation - FIX for InvalidCastException
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(254).WithMessage("Email cannot exceed 254 characters") // RFC 5321
                .MustAsync(BeUniqueEmail).WithMessage("Email is already registered");

            // Name validation
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
                .Matches("^[a-zA-Z\\s]+$").WithMessage("First name can only contain letters and spaces");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
                .Matches("^[a-zA-Z\\s]+$").WithMessage("Last name can only contain letters and spaces");

            // Password policy (enterprise-grade)
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long") // NIST recommendation
                .Must(HaveUpperCase).WithMessage("Password must contain at least one uppercase letter")
                .Must(HaveLowerCase).WithMessage("Password must contain at least one lowercase letter")
                .Must(HaveDigit).WithMessage("Password must contain at least one number")
                .Must(HaveSpecialChar).WithMessage("Password must contain at least one special character")
                .Must(NotContainUsernameOrEmail).WithMessage("Password cannot contain username or email");

            // Password confirmation
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match")
                .When(x => x.Password != null);

            // Phone validation (optional)
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format (E.164)")
                .MaximumLength(20).WithMessage("Phone number too long")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            // UserType validation
            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage("Invalid user type")
                .NotEqual(UserType.SuperAdmin).WithMessage("Cannot register as SuperAdmin");

            // Cross-field validation
            RuleFor(x => x)
                .Must(x => string.IsNullOrWhiteSpace(x.PhoneNumber) || x.PhoneNumber.Length >= 10)
                .WithMessage("Phone number must be at least 10 digits")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
            _tenantService = tenantService;
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return true;

                // FIX: Use EmailExistsAsync instead of ExistsAsync, as it matches the signature (string email, Guid tenantId, Guid? schoolId, CancellationToken)
                var exists = await _userRepository.EmailExistsAsync(email, _tenantService.TenantId, _tenantService.SchoolId, cancellationToken);
                return !exists;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking email uniqueness: {Email}", email);
                return true; // Fail open - let handler check too
            }
        }

        private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return true;

                var exists = await _userRepository.UsernameExistsAsync(username, _tenantService.TenantId, _tenantService.SchoolId, cancellationToken);
                return !exists;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking username uniqueness: {Username}", username);
                return true; // Fail open
            }
        }

        private bool HaveUpperCase(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Any(char.IsUpper);
        }

        private bool HaveLowerCase(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Any(char.IsLower);
        }

        private bool HaveDigit(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Any(char.IsDigit);
        }

        private bool HaveSpecialChar(string password)
        {
            return !string.IsNullOrWhiteSpace(password) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        private bool NotContainUsernameOrEmail(RegisterCommand command, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return true;

            var normalizedPassword = password.ToLowerInvariant();
            return !normalizedPassword.Contains(command.Username?.ToLowerInvariant() ?? "")
                && !normalizedPassword.Contains(command.Email?.ToLowerInvariant() ?? "");
        }
    }
}
