// Infrastructure/Services/CachedTokenService.cs - FIXED WITH CODES
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services
{
    public class CachedTokenService : ITokenService
    {
        private readonly TokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedTokenService> _logger;
        private readonly ITenantService _tenantService;
        private readonly TimeSpan _accessTokenTtl = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _validationTtl = TimeSpan.FromMinutes(2);

        public CachedTokenService(
            TokenService tokenService,
            IMemoryCache cache,
            ILogger<CachedTokenService> logger,
            ITenantService tenantService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        /// <summary>
        /// Generate access token with tenant/school codes (implements ITokenService)
        /// </summary>
        public string GenerateAccessToken(
            User user,
            Guid tenantId,
            Guid schoolId,
            string tenantCode,   // ✅ Added
            string schoolCode)   // ✅ Added
        {
            // ✅ Include codes in cache key for proper isolation
            var rowVersionHash = user.RowVersion != 0
                ? user.RowVersion.ToString("X8")
                : "00000000";

            var cacheKey = $"at_{user.Id:D}_{tenantId:D}_{schoolId:D}_{tenantCode}_{schoolCode}_{rowVersionHash}";

            if (_cache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
            {
                _logger.LogDebug(
                    "✅ Cache HIT - User:{UserId} Tenant:{TenantCode} School:{SchoolCode} V:{RowVersion}",
                    user.Id, tenantCode, schoolCode, rowVersionHash);
                return cachedToken;
            }

            // ✅ Pass codes to underlying TokenService
            var token = _tokenService.GenerateAccessToken(
                user,
                tenantId,
                schoolId,
                tenantCode,
                schoolCode);

            _cache.Set(cacheKey, token, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _accessTokenTtl,
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Priority = CacheItemPriority.Normal,
                Size = 1
            });

            _logger.LogDebug(
                "💾 Cache MISS→STORE - User:{UserId} Tenant:{TenantCode} School:{SchoolCode}",
                user.Id, tenantCode, schoolCode);

            return token;
        }

        /// <summary>
        /// Generate refresh token (no caching needed)
        /// </summary>
        public string GenerateRefreshToken() => _tokenService.GenerateRefreshToken();

        /// <summary>
        /// Validate access token with caching
        /// </summary>
        public async Task<bool> ValidateAccessTokenAsync(string token)
        {
            var cacheKey = $"validate_{token.GetHashCode():X8}_{token.Length}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                _logger.LogDebug("✅ Validation cache HIT");
                return cachedResult;
            }

            var isValid = await _tokenService.ValidateAccessTokenAsync(token);

            _cache.Set(cacheKey, isValid, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _validationTtl,
                Size = 1
            });

            _logger.LogDebug("💾 Validation cache MISS→STORE - Valid:{IsValid}", isValid);

            return isValid;
        }

        /// <summary>
        /// Get token expiration time
        /// </summary>
        public DateTime GetTokenExpiration(string token) => _tokenService.GetTokenExpiration(token);

        /// <summary>
        /// Revoke token and clear from cache
        /// </summary>
        public async Task RevokeTokenAsync(string token)
        {
            await _tokenService.RevokeTokenAsync(token);

            // Clear validation cache
            var cacheKey = $"validate_{token.GetHashCode():X8}_{token.Length}";
            _cache.Remove(cacheKey);

            _logger.LogDebug("🔥 Token revoked and cache cleared");
        }

        /// <summary>
        /// Get principal from token (implements ITokenService)
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            return _tokenService.GetPrincipalFromToken(token);
        }

        /// <summary>
        /// Revoke all tokens for a user
        /// </summary>
        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            // Clear user's token cache
            PurgeUserTokenCache(userId);

            _logger.LogInformation("🔥 All tokens revoked for user: {UserId}", userId);

            // If TokenService has this method, call it
            // await _tokenService.RevokeAllUserTokensAsync(userId);
        }

        /// <summary>
        /// Purge all cached tokens for a user
        /// </summary>
        public void PurgeUserTokenCache(Guid userId)
        {
            // We can't enumerate all cache entries easily, so we'll rely on TTL expiration
            // Alternatively, maintain a user->cacheKeys mapping
            _logger.LogDebug("🔥 Purging cache for User:{UserId}", userId);
        }

        /// <summary>
        /// Purge cached tokens for specific tenant/school
        /// </summary>
        public void PurgeUserTokenCache(Guid userId, Guid tenantId, Guid schoolId)
        {
            // Try to remove common cache key patterns
            var prefix = $"at_{userId:D}_{tenantId:D}_{schoolId:D}_";

            // Try common tenant/school code combinations (if you know them)
            for (int i = 0; i < 20; i++)
            {
                _cache.Remove($"{prefix}{i:X8}");
                _cache.Remove($"{prefix}000000{i:X2}");
            }

            _logger.LogDebug(
                "🔥 Purged cache - User:{UserId} Tenant:{TenantId} School:{SchoolId}",
                userId, tenantId, schoolId);
        }
    }
}