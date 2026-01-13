using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class AuthenticationTokenManager : IAuthenticationTokenManager
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly ISchoolRepository _schoolRepository; // ✅ Use repository instead of DbContext
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthenticationTokenManager> _logger;
        private readonly ICurrentUserService _currentUserService;

        public AuthenticationTokenManager(
            ITokenService tokenService,
            IUserRepository userRepository,
            ISchoolRepository schoolRepository, // ✅ Inject school repository
            IUnitOfWork unitOfWork,
            ILogger<AuthenticationTokenManager> logger,
            ICurrentUserService currentUserService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _schoolRepository = schoolRepository ?? throw new ArgumentNullException(nameof(schoolRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Generates access and refresh tokens with tenant/school context
        /// </summary>
        public async Task<TokenDto> GenerateTokensAsync(
            User user,
            string ipAddress,
            Guid tenantId,
            Guid schoolId,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "🔑 Generating tokens - User: {UserId}, Tenant: {TenantId}, School: {SchoolId}",
                    user.Id, tenantId, schoolId);

                // ✅ CLEAN ARCHITECTURE: Fetch via repository instead of DbContext
                var schoolInfo = await _schoolRepository.GetSchoolWithTenantAsync(
                    schoolId,
                    cancellationToken);

                if (schoolInfo == null)
                {
                    _logger.LogError("❌ School not found: {SchoolId}", schoolId);
                    throw new InvalidOperationException($"School {schoolId} not found");
                }

                var tenantCode = schoolInfo.TenantCode;
                var schoolCode = schoolInfo.SchoolCode;

                _logger.LogInformation(
                    "🎫 Token context - User: {UserId}, TenantCode: {TenantCode}, SchoolCode: {SchoolCode}",
                    user.Id, tenantCode, schoolCode);

                // 1. Generate access token with tenant/school codes
                var accessToken = _tokenService.GenerateAccessToken(
                    user,
                    tenantId,
                    schoolId,
                    tenantCode,    // ✅ Pass tenant code
                    schoolCode);   // ✅ Pass school code

                // 2. Generate refresh token
                var refreshTokenValue = _tokenService.GenerateRefreshToken();

                // 3. Clean expired tokens
                user.RemoveExpiredRefreshTokens();

                // 4. Add new refresh token to user
                var newToken = user.AddRefreshToken(
                    tenantId,
                    schoolId,
                    refreshTokenValue,
                    DateTime.UtcNow.AddDays(7),
                    ipAddress,
                    tokenFamily: Guid.NewGuid().ToString());

                _logger.LogInformation(
                    "➕ Created RefreshToken {TokenId} for User {UserId}",
                    newToken.Id, user.Id);

                // 5. Record successful login (no SaveChanges - handler does it)
                user.RecordSuccessfulLogin(ipAddress);

                // 6. Return token DTO
                _logger.LogInformation(
                    "✅ Tokens ready - User: {UserId}, TenantCode: {TenantCode}, SchoolCode: {SchoolCode}, RefreshToken: {TokenId}",
                    user.Id, tenantCode, schoolCode, newToken.Id);

                return new TokenDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenValue,
                    ExpiresIn = 15 * 60, // 15 minutes in seconds
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),

                    // ✅ Include codes in response for client reference
                    TenantCode = tenantCode,
                    SchoolCode = schoolCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "💥 Token generation failed - User: {UserId}, Tenant: {TenantId}, School: {SchoolId}",
                    user.Id, tenantId, schoolId);
                throw;
            }
        }

        /// <summary>
        /// Refreshes access token using valid refresh token
        /// </summary>
        public async Task<TokenDto> RefreshTokenAsync(
            string refreshToken,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🔄 Refresh token attempt from IP: {IpAddress}", ipAddress);

                // ✅ Use repository to find user with refresh token
                var user = await _userRepository.GetByRefreshTokenAsync(
                    refreshToken,
                    cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("⚠️ Invalid refresh token attempted from IP: {IpAddress}", ipAddress);
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }

                var oldToken = user.RefreshTokens
                    .FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);

                if (oldToken == null || !oldToken.IsActive)
                {
                    _logger.LogWarning(
                        "⚠️ Expired or revoked refresh token - User: {UserId}, IP: {IpAddress}",
                        user.Id, ipAddress);
                    throw new UnauthorizedAccessException("Refresh token expired or revoked");
                }

                // Validate school assignment
                if (!user.SchoolId.HasValue)
                {
                    _logger.LogError("❌ User {UserId} has no school assignment", user.Id);
                    throw new InvalidOperationException("User has no school assignment");
                }

                // Get school info for new tokens
                var schoolInfo = await _schoolRepository.GetSchoolWithTenantAsync(
                    user.SchoolId.Value,
                    cancellationToken);

                if (schoolInfo == null)
                {
                    _logger.LogError("❌ School not found for user: {UserId}", user.Id);
                    throw new InvalidOperationException("School not found for user");
                }

                // Revoke old token
                //oldToken.RevokedAt = DateTime.UtcNow;
                //oldToken.RevokedByIp = ipAddress;

                _logger.LogInformation(
                    "🔄 Refreshing tokens - User: {UserId}, OldToken: {OldTokenId}",
                    user.Id, oldToken.Id);

                // Generate new tokens
                var newTokens = await GenerateTokensAsync(
                    user,
                    ipAddress,
                    schoolInfo.TenantId,
                    user.SchoolId.Value,
                    cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Token refresh successful - User: {UserId}, NewRefreshToken generated",
                    user.Id);

                return newTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Token refresh failed");
                throw;
            }
        }
    }
}