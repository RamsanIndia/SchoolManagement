using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
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
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _secretKey = _configuration["Jwt:SecretKey"];
            _issuer = _configuration["Jwt:Issuer"];
            _audience = _configuration["Jwt:Audience"];
        }

        public string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey); // ✅ Use UTF8, not ASCII

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
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

                    new Claim(ClaimTypes.Role, user.UserType.ToString()),
                    
                    // Add unique token identifier for revocation
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // ✅ Shorter expiry (15 min)
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
                "Access token generated for user {UserId}. Expires at {Expiry}",
                user.Id,
                tokenDescriptor.Expires);

            return tokenString;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64]; // ✅ Increased from 32 to 64 bytes
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<bool> ValidateAccessTokenAsync(string token)
        {
            try
            {
                // ✅ Check if token is blacklisted
                var jti = GetTokenJti(token);
                var isRevoked = await IsTokenRevokedAsync(jti);
                if (isRevoked)
                {
                    _logger.LogWarning("Attempted use of revoked token {Jti}", jti);
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
                    ClockSkew = TimeSpan.Zero, // ✅ No clock skew tolerance

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                });

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token validation failed: Token expired");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return false;
            }
        }

        // ✅ Add token revocation
        public async Task RevokeTokenAsync(string token)
        {
            var jti = GetTokenJti(token);
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

                _logger.LogInformation("Token {Jti} revoked until {Expiration}", jti, expiration);
            }
        }

        private async Task<bool> IsTokenRevokedAsync(string jti)
        {
            var value = await _cache.GetStringAsync($"blacklist:{jti}");
            return value != null;
        }

        private string GetTokenJti(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }

        public DateTime GetTokenExpiration(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            return jwt.ValidTo;
        }
    }
}
