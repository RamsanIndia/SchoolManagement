using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    /// <summary>
    /// Login command handler with proper DbContext sharing
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

                _logger.LogInformation("🔵 LOGIN ATTEMPT for email: {Email}", request.Email);

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

                // Get user with pessimistic lock
                // Now IUserRepository shares the same DbContext as IUnitOfWork!
                var user = await _userRepository.GetByEmailWithLockAsync(email.Value, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("❌ Login attempt with non-existent email: {Email}", email.Value);
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                _logger.LogInformation("✅ User found: {UserId}, Username: {Username}", user.Id, user.Username);

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

                    // Record failed login
                    user.RecordFailedLogin(MaxLoginAttempts, LockoutDuration);
                    await _userRepository.UpdateAsync(user, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // Successful login
                var result = await HandleSuccessfulLoginAsync(user, ipAddress, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure("An error occurred during login. Please try again.");
            }
        }

        private async Task<Result<AuthResponseDto>> HandleSuccessfulLoginAsync(
            User user, string ipAddress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🟢 HANDLING SUCCESSFUL LOGIN for user: {UserId}", user.Id);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("🔑 Generated tokens for user: {UserId}", user.Id);

            // 🔍 DEBUG: Check before adding token
            _logger.LogInformation("🔍 BEFORE - RefreshTokens count: {Count}", user.RefreshTokens.Count);

            // Clean up expired tokens
            user.RemoveExpiredRefreshTokens();
            _logger.LogInformation("🧹 After cleanup - RefreshTokens count: {Count}", user.RefreshTokens.Count);

            // Add new refresh token
            var newToken = user.AddRefreshToken(
                refreshTokenValue,
                DateTime.UtcNow.AddDays(7),
                ipAddress
            );

            _logger.LogInformation("➕ Added token - ID: {TokenId}, UserId: {UserId}", newToken.Id, newToken.UserId);
            _logger.LogInformation("🔍 AFTER adding - RefreshTokens count: {Count}", user.RefreshTokens.Count);

            // Record successful login
            user.RecordSuccessfulLogin();

            // 🔍 DEBUG: Log change tracker before save
            //_logger.LogInformation("📊 CHANGE TRACKER BEFORE SAVE:");
            //var trackedEntities = _unitOfWork.GetTrackedEntitiesDebugInfo();
            //foreach (var entity in trackedEntities)
            //{
            //    _logger.LogInformation("   {EntityInfo}", entity);
            //}

            // Update and save
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("💾 Calling SaveChangesAsync...");
            var savedCount = await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("✅ SaveChangesAsync completed. Saved {SavedCount} changes.", savedCount);

            // 🔍 DEBUG: Verify token in database
            //var tokenExists = await _unitOfWork.RefreshTokenExistsInDatabaseAsync(refreshTokenValue, cancellationToken);
            //if (tokenExists)
            //{
            //    _logger.LogInformation("✅✅✅ SUCCESS! Token found in database!");
            //}
            //else
            //{
            //    _logger.LogError("❌❌❌ FAILURE! Token NOT found in database!");
            //}

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