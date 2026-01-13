using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TokenService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(
            IConfiguration configuration,
            IDistributedCache cache,
            ILogger<TokenService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
            _issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer not configured");
            _audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience not configured");
        }

        /// <summary>
        /// Generates JWT access token with tenant and school context
        /// </summary>
        public string GenerateAccessToken(
            User user,
            Guid tenantId,
            Guid schoolId,
            string tenantCode,  // ✅ Added
            string schoolCode)  // ✅ Added
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // ✅ User identity claims
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim("userId", user.Id.ToString()),

                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("username", user.Username),

                    new Claim(ClaimTypes.GivenName, user.FullName.FirstName),
                    new Claim(ClaimTypes.Surname, user.FullName.LastName),
                    new Claim("fullName", $"{user.FullName.FirstName} {user.FullName.LastName}"),

                    new Claim(ClaimTypes.Email, user.Email.Value),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),

                    // ✅ CRITICAL: Tenant and School CODES for middleware
                    new Claim("TenantCode", tenantCode),
                    new Claim("SchoolCode", schoolCode),
                    
                    // ✅ IDs for backward compatibility and queries
                    new Claim("TenantId", tenantId.ToString()),
                    new Claim("SchoolId", schoolId.ToString()),
                    new Claim("SchoolName", user.School?.Name ?? schoolCode ?? "Unknown"),

                    // ✅ User type/role
                    new Claim("UserType", user.UserType.ToString()),
                    new Claim(ClaimTypes.Role, user.UserType.ToString()),

                    // ✅ Security claims
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),  // ✅ 15 minutes for better security
                NotBefore = DateTime.UtcNow,
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation(
                "🔑 Access token generated - User: {UserId}, Tenant: {TenantCode}, School: {SchoolCode}, Expires: {Expiry}",
                user.Id,
                tenantCode,
                schoolCode,
                tokenDescriptor.Expires);

            return tokenString;
        }

        /// <summary>
        /// Generates cryptographically secure refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber);

            _logger.LogDebug("🔄 Refresh token generated");

            return refreshToken;
        }

        /// <summary>
        /// Validates access token and checks if revoked
        /// </summary>
        public async Task<bool> ValidateAccessTokenAsync(string token)
        {
            try
            {
                // ✅ Check if token is blacklisted
                var jti = GetTokenJti(token);
                var isRevoked = await IsTokenRevokedAsync(jti);

                if (isRevoked)
                {
                    _logger.LogWarning("⚠️ Attempted use of revoked token {Jti}", jti);
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // ✅ No tolerance for expired tokens

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                });

                _logger.LogDebug("✅ Token validated successfully");
                return true;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("⚠️ Token validation failed: Token expired - {Message}", ex.Message);
                return false;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("⚠️ Token validation failed: Invalid token - {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Token validation failed with unexpected error");
                return false;
            }
        }

        /// <summary>
        /// Revokes a token by adding it to blacklist cache
        /// </summary>
        public async Task RevokeTokenAsync(string token)
        {
            try
            {
                var jti = GetTokenJti(token);

                if (string.IsNullOrEmpty(jti))
                {
                    _logger.LogWarning("⚠️ Cannot revoke token: JTI not found");
                    return;
                }

                var expiration = GetTokenExpiration(token);
                var ttl = expiration - DateTime.UtcNow;

                if (ttl > TimeSpan.Zero)
                {
                    await _cache.SetStringAsync(
                        $"blacklist:{jti}",
                        "revoked",
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpiration = expiration
                        });

                    _logger.LogInformation(
                        "🚫 Token revoked - JTI: {Jti}, Expires: {Expiration}",
                        jti, expiration);
                }
                else
                {
                    _logger.LogDebug("Token already expired, no need to blacklist - JTI: {Jti}", jti);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Failed to revoke token");
                throw;
            }
        }

        /// <summary>
        /// Checks if token is in blacklist cache
        /// </summary>
        private async Task<bool> IsTokenRevokedAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti))
            {
                return false;
            }

            try
            {
                var value = await _cache.GetStringAsync($"blacklist:{jti}");
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Failed to check token revocation status");
                // Fail open - assume not revoked if cache fails
                return false;
            }
        }

        /// <summary>
        /// Extracts JTI claim from token
        /// </summary>
        private string GetTokenJti(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract JTI from token");
                return null;
            }
        }

        /// <summary>
        /// Gets token expiration time
        /// </summary>
        public DateTime GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                return jwt.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract expiration from token");
                return DateTime.UtcNow; // Return current time as fallback
            }
        }

        /// <summary>
        /// Extracts claims principal from token (useful for debugging)
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = false, // Don't validate expiration when extracting claims
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract principal from token");
                return null;
            }
        }
    }
}