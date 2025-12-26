namespace SchoolManagement.Application.Auth.Handler
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using SchoolManagement.Application.Auth.Commands;
    using SchoolManagement.Application.DTOs;
    using SchoolManagement.Application.Interfaces;
    using SchoolManagement.Domain.Common;
    using SchoolManagement.Domain.ValueObjects;
    using System;
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
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IAccountSecurityService securityService,
            IAuthenticationTokenManager tokenManager,
            IAuthResponseBuilder responseBuilder,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _securityService = securityService;
            _tokenManager = tokenManager;
            _responseBuilder = responseBuilder;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

                _logger.LogInformation("🔵 LOGIN ATTEMPT for email: {Email}", request.Email);

                // Validate email format
                var emailResult = ValidateEmail(request.Email);
                if (!emailResult.Status)
                {
                    return Result<AuthResponseDto>.Failure(emailResult.Message);
                }

                // Get user
                var user = await _userRepository.GetByEmailWithLockAsync(emailResult.Data.Value, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("❌ Login attempt with non-existent email: {Email}", request.Email);
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                _logger.LogInformation("✅ User found: {UserId}, Username: {Username}", user.Id, user.Username);

                // Validate account status
                var statusResult = await _securityService.ValidateAccountStatusAsync(user);
                if (!statusResult.Status)
                {
                    return Result<AuthResponseDto>.Failure(statusResult.Message);
                }

                // Verify credentials
                var credentialsResult = await _securityService.VerifyCredentialsAsync(user, request.Password);
                if (!credentialsResult.Status)
                {
                    await _securityService.HandleFailedLoginAsync(user, cancellationToken);
                    return Result<AuthResponseDto>.Failure(credentialsResult.Message);
                }

                // Generate tokens
                var tokenResult = await _tokenManager.GenerateTokensAsync(user, ipAddress, cancellationToken);

                // Build response
                var response = _responseBuilder.BuildAuthResponse(user, tokenResult.AccessToken, tokenResult.RefreshToken);

                _logger.LogInformation("🟢 Successful login for user: {UserId}", user.Id);

                return Result<AuthResponseDto>.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during login for email: {Email}", request.Email);
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
            catch (ArgumentException)
            {
                _logger.LogWarning("Invalid email format attempted: {Email}", email);
                return Result<Email>.Failure("Invalid email format");
            }
        }
    }
}