using SchoolManagement.Domain.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    /// <summary>
    /// Service for JWT token generation and validation
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates JWT access token with tenant/school context
        /// </summary>
        /// <param name="user">The user to generate token for</param>
        /// <param name="tenantId">Tenant ID (GUID)</param>
        /// <param name="schoolId">School ID (GUID)</param>
        /// <param name="tenantCode">Tenant code (string) - for middleware extraction</param>
        /// <param name="schoolCode">School code (string) - for middleware extraction</param>
        /// <returns>JWT access token string</returns>
        string GenerateAccessToken(
            User user,
            Guid tenantId,
            Guid schoolId,
            string tenantCode,  // ✅ Added
            string schoolCode); // ✅ Added

        /// <summary>
        /// Generates cryptographically secure refresh token
        /// </summary>
        /// <returns>Base64 encoded refresh token</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates JWT access token signature, expiration, and revocation status
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateAccessTokenAsync(string token);

        /// <summary>
        /// Revokes a token by adding it to the blacklist cache
        /// </summary>
        /// <param name="token">Token to revoke</param>
        Task RevokeTokenAsync(string token);

        /// <summary>
        /// Gets the expiration datetime of a token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Expiration datetime in UTC</returns>
        DateTime GetTokenExpiration(string token);

        /// <summary>
        /// Extracts claims principal from token without validating expiration
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>ClaimsPrincipal containing token claims</returns>
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}