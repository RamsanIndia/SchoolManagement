// Application/Auth/Handlers/LoginCommandHandler.cs - MULTI-TENANT CORRECTED
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handlers
{
    /// <summary>
    /// Orchestrates secure tenant-aware login with token rotation and multi-tenant context override
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountSecurityService _securityService;
        private readonly IAuthenticationTokenManager _tokenManager;
        private readonly IAuthResponseBuilder _responseBuilder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantService _tenantService;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IAccountSecurityService securityService,
            IAuthenticationTokenManager tokenManager,
            IAuthResponseBuilder responseBuilder,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            ILogger<LoginCommandHandler> logger,
            ICurrentUserService currentUserService,
            ITenantService tenantService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
            _responseBuilder = responseBuilder ?? throw new ArgumentNullException(nameof(responseBuilder));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
        {
            try
            {
                // Get initial tenant context from middleware (may be defaults)
                var middlewareTenantId = _tenantService.TenantId;
                var clientIp = _currentUserService.IpAddress ?? "Unknown";

                _logger.LogInformation(
                    "🔵 Login attempt - Email: {Email}, Middleware Tenant: {TenantId}, IP: {IP}",
                    request.Email, middlewareTenantId, clientIp);

                // STEP 1: Validate email format
                var emailResult = ValidateEmail(request.Email);
                if (!emailResult.Status)
                {
                    _logger.LogWarning("❌ Invalid email format: {Email}", request.Email);
                    return Result<AuthResponseDto>.Failure(emailResult.Message);
                }

                // STEP 2: Get user by email within tenant context
                var user = await _userRepository.GetByEmailAsync(
                    emailResult.Data.Value,
                    middlewareTenantId,
                    ct);

                if (user == null)
                {
                    _logger.LogWarning(
                        "❌ User not found - Email: {Email}, Tenant: {TenantId}",
                        request.Email, middlewareTenantId);

                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // STEP 3: Validate user has school assignment
                if (!user.SchoolId.HasValue || user.SchoolId == Guid.Empty)
                {
                    _logger.LogError(
                        "❌ User {UserId} has no school assignment",
                        user.Id);

                    return Result<AuthResponseDto>.Failure(
                        "Your account is not assigned to a school. Please contact your administrator.");
                }

                _logger.LogInformation(
                    "✅ User found - UserId: {UserId}, Username: {Username}, DB TenantId: {TenantId}, DB SchoolId: {SchoolId}, UserType: {UserType}",
                    user.Id, user.Username, user.TenantId, user.SchoolId.Value, user.UserType);

                // ✅ STEP 3.5: CRITICAL FIX - Override middleware defaults with user's actual tenant/school from DB
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var oldTenantId = httpContext.Items["TenantId"];
                    var oldSchoolId = httpContext.Items["SchoolId"];

                    // Set user's ACTUAL tenant/school from database
                    httpContext.Items["TenantId"] = user.TenantId;
                    httpContext.Items["SchoolId"] = user.SchoolId.Value;

                    // Optional: Store codes if available
                    //if (!string.IsNullOrEmpty(user.SchoolCode))
                    //{
                    //    httpContext.Items["SchoolCode"] = user.SchoolCode;
                    //}

                    _logger.LogInformation(
                        "🔄 Tenant/School context override - User {UserId} ({Email}): " +
                        "TenantId: {OldTenant} → {NewTenant}, SchoolId: {OldSchool} → {NewSchool}",
                        user.Id, user.Email.Value,
                        oldTenantId, user.TenantId,
                        oldSchoolId, user.SchoolId.Value);
                }
                else
                {
                    _logger.LogWarning("⚠️ HttpContext is null - cannot override tenant context");
                }

                // STEP 4: Validate account status (active, not locked, not deleted)
                var statusResult = await _securityService.ValidateAccountStatusAsync(user);
                if (!statusResult.Status)
                {
                    _logger.LogWarning(
                        "⚠️ Account status validation failed - User: {UserId}, Reason: {Reason}",
                        user.Id, statusResult.Message);

                    return Result<AuthResponseDto>.Failure(statusResult.Message);
                }

                // STEP 5: Verify password credentials
                var credsResult = await _securityService.VerifyCredentialsAsync(user, request.Password);
                if (!credsResult.Status)
                {
                    _logger.LogWarning(
                        "❌ Invalid password - User: {UserId}, Email: {Email}",
                        user.Id, user.Email.Value);

                    // Record failed login attempt (may lock account)
                    await _securityService.HandleFailedLoginAsync(user, ct);

                    // Save failed login attempt
                    await _unitOfWork.SaveChangesAsync(ct);

                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // STEP 6: Update login status (NO SAVE YET)
                user.RecordSuccessfulLogin(clientIp);
                user.RemoveExpiredRefreshTokens();

                // Update repository tracking
                await _userRepository.UpdateAsync(user, ct);

                // STEP 7: Generate tokens with user's actual tenant/school
                // TokenManager will now use overridden context values
                var tokenResult = await _tokenManager.GenerateTokensAsync(
                    user,
                    clientIp,
                    (Guid)user.TenantId,           // ✅ Use user's actual TenantId
                    user.SchoolId.Value,      // ✅ Use user's actual SchoolId
                    ct);

                if (tokenResult == null)
                {
                    _logger.LogError(
                        "❌ Token generation failed - User: {UserId}",
                        user.Id);

                    return Result<AuthResponseDto>.Failure("Failed to generate authentication tokens");
                }

                // STEP 8: Save everything in ONE transaction
                // This saves both the user update AND the refresh token that was added
                await _unitOfWork.SaveChangesAsync(ct);

                // STEP 9: Build response
                var response = _responseBuilder.BuildAuthResponse(
                    user,
                    tokenResult.AccessToken,
                    tokenResult.RefreshToken);

                _logger.LogInformation(
                    "✅ Login successful - UserId: {UserId}, Email: {Email}, TenantId: {TenantId}, SchoolId: {SchoolId}",
                    user.Id, user.Email.Value, user.TenantId, user.SchoolId.Value);

                return Result<AuthResponseDto>.Success(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex,
                    "⚠️ Login failed due to invalid operation - Email: {Email}",
                    request.Email);

                return Result<AuthResponseDto>.Failure(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex,
                    "⚠️ Unauthorized access attempt - Email: {Email}",
                    request.Email);

                return Result<AuthResponseDto>.Failure("Access denied");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "💥 Unexpected error during login - Email: {Email}",
                    request.Email);

                return Result<AuthResponseDto>.Failure(
                    "An error occurred during login. Please try again later.");
            }
        }

        /// <summary>
        /// Validate email format and create Email value object
        /// </summary>
        private Result<Email> ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<Email>.Failure("Email is required");
            }

            try
            {
                var emailValueObject = new Email(email);
                return Result<Email>.Success(emailValueObject);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    "Invalid email format - Email: {Email}, Error: {Error}",
                    email, ex.Message);

                return Result<Email>.Failure("Invalid email format");
            }
        }
    }
}