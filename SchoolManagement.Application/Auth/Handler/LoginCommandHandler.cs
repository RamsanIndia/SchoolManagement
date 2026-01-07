namespace SchoolManagement.Application.Auth.Handler
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using SchoolManagement.Application.Auth.Commands;
    using SchoolManagement.Application.DTOs;
    using SchoolManagement.Application.Interfaces;
    using SchoolManagement.Application.Shared.Utilities;
    using SchoolManagement.Domain.Common;
    using SchoolManagement.Domain.ValueObjects;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// SRP: Only orchestrates the login process
    /// OCP: Open for extension through service injection
    /// DIP: Depends on abstractions
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountSecurityService _securityService;
        private readonly IAuthenticationTokenManager _tokenManager;
        private readonly IAuthResponseBuilder _responseBuilder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IAccountSecurityService securityService,
            IAuthenticationTokenManager tokenManager,
            IAuthResponseBuilder responseBuilder,
            IHttpContextAccessor httpContextAccessor,
            IpAddressHelper ipAddressHelper,
            IUnitOfWork unitOfWork,
            ILogger<LoginCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
            _tokenManager = tokenManager;
            _responseBuilder = responseBuilder;
            _httpContextAccessor = httpContextAccessor;
            _ipAddressHelper = ipAddressHelper;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AuthResponseDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var clientIp = _currentUserService.IpAddress;

                _logger.LogInformation("🔵 LOGIN ATTEMPT for email: {Email} from IP: {IP}", request.Email, clientIp);

                // Validate email format
                var emailResult = ValidateEmail(request.Email);
                if (!emailResult.Status)
                {
                    return Result<AuthResponseDto>.Failure(emailResult.Message);
                }

                // Get user with all refresh tokens for management
                var user = await _userRepository.GetByEmailWithTokensAsync(
                    emailResult.Data.Value,
                    cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("❌ Login attempt with non-existent email: {Email} from IP: {IP}",
                        request.Email, clientIp);
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                _logger.LogInformation("✅ User found: {UserId}, Username: {Username}", user.Id, user.Username);

                // Validate account status
                var statusResult = await _securityService.ValidateAccountStatusAsync(user);
                if (!statusResult.Status)
                {
                    _logger.LogWarning("⚠️ Account status validation failed for user {UserId}: {Reason}",
                        user.Id, statusResult.Message);
                    return Result<AuthResponseDto>.Failure(statusResult.Message);
                }

                // Verify credentials
                var credentialsResult = await _securityService.VerifyCredentialsAsync(user, request.Password);
                if (!credentialsResult.Status)
                {
                    _logger.LogWarning("❌ Invalid credentials for user {UserId}", user.Id);
                    await _securityService.HandleFailedLoginAsync(user, cancellationToken);
                    return Result<AuthResponseDto>.Failure(credentialsResult.Message);
                }

                // 🔒 SECURITY: Check for too many active sessions before creating new token
                if (user.HasTooManySessions(maxSessions: 5))
                {
                    _logger.LogWarning(
                        "⚠️ User {UserId} has {Count} active sessions. Consider revoking oldest sessions.",
                        user.Id,
                        user.GetActiveTokenCount());

                    // Optional: Auto-revoke oldest sessions if limit exceeded
                    // var excessCount = user.GetActiveTokenCount() - 4; // Keep 4, make room for 1 new
                    // var oldestTokens = user.GetActiveRefreshTokens()
                    //     .OrderBy(t => t.CreatedDate)
                    //     .Take(excessCount)
                    //     .ToList();
                    // 
                    // foreach (var oldToken in oldestTokens)
                    // {
                    //     user.RevokeRefreshToken(oldToken.Id, clientIp, "Session limit exceeded - oldest session removed");
                    // }
                }

                // Generate tokens with IP tracking
                var tokenResult = await _tokenManager.GenerateTokensAsync(user, clientIp, cancellationToken);

                // Update last login and reset failed attempts
                user.RecordSuccessfulLogin();

                // Clean up expired tokens (older than 30 days)
                user.RemoveExpiredRefreshTokens();

                // Save all changes
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Build response
                var response = _responseBuilder.BuildAuthResponse(
                    user,
                    tokenResult.AccessToken,
                    tokenResult.RefreshToken);

                _logger.LogInformation(
                    "🟢 Successful login for user: {UserId}, Active sessions: {SessionCount}",
                    user.Id,
                    user.GetActiveTokenCount());

                return Result<AuthResponseDto>.Success(response, "Login successful");
            }
            catch (InvalidOperationException ex)
            {
                // Domain-specific exceptions (e.g., account locked, inactive user)
                _logger.LogWarning(ex, "⚠️ Domain validation error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure("An error occurred during login. Please try again.");
            }
        }

        private Result<Email> ValidateEmail(string email)
        {
            try
            {
                var emailObj = new Email(email);
                return Result<Email>.Success(emailObj, "Email validated");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid email format attempted: {Email}, Error: {Error}", email, ex.Message);
                return Result<Email>.Failure("Invalid email format");
            }
        }
    }
}
