using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class AccountSecurityService : IAccountSecurityService
    {
        private readonly IPasswordService _passwordService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountSecurityService> _logger;

        private const int MaxLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public AccountSecurityService(
            IPasswordService passwordService,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<AccountSecurityService> logger)
        {
            _passwordService = passwordService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Task<Result> ValidateAccountStatusAsync(User user)
        {
            if (user.IsLockedOut())
            {
                _logger.LogWarning("Login attempt for locked account: {UserId}", user.Id);
                return Task.FromResult(Result.Failure("Account is locked. Please try again later."));
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for deactivated account: {UserId}", user.Id);
                return Task.FromResult(Result.Failure("Account is deactivated. Please contact support."));
            }

            return Task.FromResult(Result.Success("Account status validated."));
        }

        public Task<Result> VerifyCredentialsAsync(User user, string password)
        {
            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);
                return Task.FromResult(Result.Failure("Invalid email or password"));
            }

            return Task.FromResult(Result.Success("Credentials verified."));
        }

        public async Task HandleFailedLoginAsync(User user, CancellationToken cancellationToken)
        {
            user.RecordFailedLogin(MaxLoginAttempts, LockoutDuration);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
