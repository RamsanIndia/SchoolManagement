using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    /// <summary>
    /// Login command handler with pessimistic locking to prevent concurrency conflicts
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const int MaxLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public LoginCommandHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ITokenService tokenService,
            ILogger<LoginCommandHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

                // Validate email format
                Email email;
                try
                {
                    email = new Email(request.Email);
                }
                catch (ArgumentException)
                {
                    _logger.LogWarning("Invalid email format attempted: {Email}", request.Email);
                    return Result<AuthResponseDto>.Failure("Invalid email format");
                }

                // Get user with pessimistic lock (blocks concurrent logins)
                var user = await _userRepository.GetByEmailWithLockAsync(email.Value, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", email.Value);
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // Check if account is locked
                if (user.IsLockedOut())
                {
                    _logger.LogWarning("Login attempt for locked account: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Account is locked. Please try again later.");
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for deactivated account: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Account is deactivated. Please contact support.");
                }

                // Verify password
                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);

                    // Apply failed login attempt logic
                    user.RecordFailedLogin(MaxLoginAttempts, LockoutDuration);

                    await _userRepository.UpdateAsync(user, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // Successful login
                var result = await HandleSuccessfulLoginAsync(user, ipAddress, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure("An error occurred during login. Please try again.");
            }
        }

        /// <summary>
        /// Handle successful login - no concurrency issues since we have pessimistic lock
        /// </summary>
        private async Task<Result<AuthResponseDto>> HandleSuccessfulLoginAsync(User user, string ipAddress, CancellationToken cancellationToken)
        {
            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();

            // Clean up expired tokens to reduce DB bloat
            user.RemoveExpiredRefreshTokens();
            
            // Add new refresh token (raises RefreshTokenCreatedEvent)
            user.AddRefreshToken(
                refreshTokenValue,
                DateTime.UtcNow.AddDays(7),
                ipAddress
            );

            // Record successful login (raises UserLoggedInEvent)
            user.RecordSuccessfulLogin();

            // Update and save
            await _userRepository.UpdateAsync(user, cancellationToken);

            var savedCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "✅ Successful login for user: {UserId} from IP: {IpAddress}. Saved {SavedCount} changes.",
                user.Id, ipAddress, savedCount);

            // Build response
            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email.Value,
                    FirstName = user.FullName.FirstName,
                    LastName = user.FullName.LastName,
                    PhoneNumber = user.PhoneNumber?.Value,
                    IsEmailVerified = user.EmailVerified,
                    IsPhoneVerified = user.PhoneVerified,
                    Roles = new List<string> { user.UserType.ToString() }
                }
            };

            return Result<AuthResponseDto>.Success(response);
        }
    }
}