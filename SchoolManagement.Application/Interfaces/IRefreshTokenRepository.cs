// Application/Interfaces/IRefreshTokenRepository.cs
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        // Basic CRUD operations
        Task<RefreshToken> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        // Token retrieval methods
        /// <summary>
        /// Gets an ACTIVE refresh token by token value
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// 🔒 SECURITY: Gets a REVOKED token to detect replay attacks
        /// </summary>
        Task<RefreshToken?> GetRevokedTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// 🔒 SECURITY: Gets all tokens in a token family
        /// </summary>
        Task<List<RefreshToken>> GetTokenFamilyAsync(string tokenFamily, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tokens for a user (including revoked)
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active tokens for a user
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 🔒 SECURITY: Gets active tokens count for session limiting
        /// </summary>
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Token management methods
        /// <summary>
        /// Deletes expired tokens older than retention period
        /// </summary>
        Task DeleteExpiredTokensAsync(int retentionDays = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// 🔒 SECURITY: Revokes all tokens for a user (logout all devices)
        /// </summary>
        Task RevokeAllTokensForUserAsync(Guid userId, string revokedByIp, string reason = "Logout all devices", CancellationToken cancellationToken = default);

        /// <summary>
        /// 🔒 SECURITY: Revokes entire token family (security breach response)
        /// </summary>
        Task RevokeTokenFamilyAsync(string tokenFamily, string revokedByIp, string reason = "Token family compromised", CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets token statistics for monitoring
        /// </summary>
        Task<RefreshTokenStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
