// Application/Auth/Handlers/RefreshTokenCommandHandler.cs - MULTI-TENANT COMPLETE
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ISchoolRepository _schoolRepository;  // ✅ Added
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly ITenantService _tenantService;
        private readonly ICurrentUserService _currentUserService;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ISchoolRepository schoolRepository,  // ✅ Added
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<RefreshTokenCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IpAddressHelper ipAddressHelper,
            ICurrentUserService currentUserService,
            ITenantService tenantService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _schoolRepository = schoolRepository ?? throw new ArgumentNullException(nameof(schoolRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _ipAddressHelper = ipAddressHelper ?? throw new ArgumentNullException(nameof(ipAddressHelper));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            try
            {
                // ✅ RESOLVE CONTEXT
                var tenantId = _tenantService.TenantId;
                var schoolId = _tenantService.SchoolId ?? Guid.Empty;
                var clientIp = _currentUserService.IpAddress;

                _logger.LogInformation("🔄 Refresh attempt. Token: {TokenHash}, Tenant: {TenantId}, School: {SchoolId}",
                    request.RefreshToken[..8], tenantId, schoolId);

                // ✅ VALIDATE INPUT
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    _logger.LogWarning("Empty refresh token");
                    return Result<AuthResponseDto>.Failure("Refresh token required");
                }

                // ✅ TENANT-ISOLATED TOKEN LOOKUP
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
                    request.RefreshToken, tenantId, schoolId, ct);

                if (refreshToken == null)
                {
                    _logger.LogWarning("Token not found. Tenant: {TenantId}, School: {SchoolId}", tenantId, schoolId);

                    // 🔒 SECURITY: Check revoked tokens
                    var revokedToken = await _refreshTokenRepository.GetRevokedTokenAsync(
                        request.RefreshToken, tenantId, schoolId, ct);

                    if (revokedToken != null)
                    {
                        _logger.LogError("🚨 REUSE DETECTED! Token: {TokenId}, Tenant: {TenantId}",
                            revokedToken.Id, tenantId);

                        await HandleSecurityBreachAsync(revokedToken.UserId, tenantId, schoolId, clientIp, ct);
                        return Result<AuthResponseDto>.Failure("Security violation. Login again.");
                    }

                    return Result<AuthResponseDto>.Failure("Invalid refresh token");
                }

                // ✅ VALIDATE TOKEN STATE
                refreshToken.ValidateActive();

                // ✅ LOAD USER WITH TOKENS (tenant-scoped)
                var user = await _userRepository.GetByIdWithTokensAsync(
                    refreshToken.UserId, tenantId, schoolId, ct);

                if (user == null || user.TenantId != tenantId || user.SchoolId != schoolId)
                {
                    _logger.LogWarning("User mismatch. TokenUser: {UserId}, Expected Tenant: {TenantId}",
                        refreshToken.UserId, tenantId);
                    return Result<AuthResponseDto>.Failure("Invalid token");
                }

                // ✅ ACCOUNT STATUS
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user refresh. User: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Account deactivated");
                }

                if (user.IsLockedOut)
                {
                    _logger.LogWarning("Locked user refresh. User: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Account locked");
                }

                // ✅ SESSION LIMIT
                if (user.HasTooManySessions(5))
                    _logger.LogWarning("⚠️ High sessions. User: {UserId}, Count: {Count}",
                        user.Id, user.GetActiveTokenCount());

                // ✅ GET SCHOOL WITH TENANT INFO
                var schoolInfo = await _schoolRepository.GetSchoolWithTenantAsync(schoolId, ct);

                if (schoolInfo == null)
                {
                    _logger.LogError("❌ School not found: {SchoolId}", schoolId);
                    return Result<AuthResponseDto>.Failure("School information not found");
                }

                _logger.LogInformation(
                    "🎫 Generating tokens - User: {UserId}, Tenant: {TenantCode}, School: {SchoolCode}",
                    user.Id, schoolInfo.TenantCode, schoolInfo.SchoolCode);

                // ✅ ROTATE TOKENS (with tenant/school codes)
                var newAccessToken = _tokenService.GenerateAccessToken(
                    user,
                    tenantId,
                    schoolId,
                    schoolInfo.TenantCode,   // ✅ Added
                    schoolInfo.SchoolCode);  // ✅ Added

                var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

                // Revoke old
                user.RevokeRefreshToken(refreshToken.Id, clientIp, "Rotated");

                // Add new (same family)
                var newRefreshToken = user.AddRefreshToken(
                    tenantId,
                    schoolId,
                    newRefreshTokenValue,
                    DateTime.UtcNow.AddDays(7),
                    clientIp,
                    refreshToken.TokenFamily);

                // Update state
                user.RecordSuccessfulLogin(_currentUserService.IpAddress);
                user.RemoveExpiredRefreshTokens();

                // ✅ PERSIST (tenant-validated)
                await _userRepository.UpdateAsync(user, tenantId, schoolId, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "🟢 Refresh success - User: {UserId}, Tenant: {TenantCode}, School: {SchoolCode}",
                    user.Id, schoolInfo.TenantCode, schoolInfo.SchoolCode);

                // ✅ RESPONSE (with tenant/school codes)
                return Result<AuthResponseDto>.Success(
                    BuildResponse(user, newAccessToken, newRefreshTokenValue, schoolInfo));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Domain error refresh");
                return Result<AuthResponseDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh error");
                return Result<AuthResponseDto>.Failure("Refresh failed. Login again.");
            }
        }

        /// <summary>
        /// 🔒 Handle security breach (token reuse)
        /// </summary>
        private async Task HandleSecurityBreachAsync(
            Guid userId,
            Guid tenantId,
            Guid schoolId,
            string clientIp,
            CancellationToken ct)
        {
            var compromisedUser = await _userRepository.GetByIdWithTokensAsync(userId, tenantId, schoolId, ct);
            if (compromisedUser != null)
            {
                // Revoke entire token family
                compromisedUser.RevokeTokenFamily(null, clientIp, "Security breach detected");
                await _userRepository.UpdateAsync(compromisedUser, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        /// <summary>
        /// Build auth response with tenant/school information
        /// </summary>
        private static AuthResponseDto BuildResponse(
            User user,
            string accessToken,
            string refreshToken,
            SchoolWithTenantDto schoolInfo)
        {
            return new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900, // 15min
                TokenType = "Bearer",

                // ✅ Add tenant/school codes
                TenantCode = schoolInfo.TenantCode,
                SchoolCode = schoolInfo.SchoolCode,
                SchoolName = schoolInfo.SchoolName,

                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email.Value,
                    FirstName = user.FullName.FirstName,
                    LastName = user.FullName.LastName,
                    PhoneNumber = user.PhoneNumber?.Value,
                    IsEmailVerified = user.EmailVerified,
                    IsPhoneVerified = user.PhoneVerified,
                    SchoolId = user.SchoolId.ToString(),
                    Roles = new List<string> { user.UserType.ToString() }
                }
            };
        }
    }
}