// Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly SchoolManagementDbContext _context;

        public RefreshTokenRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a refresh token by ID
        /// </summary>
        public async Task<RefreshToken> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Id == id && !rt.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Gets an ACTIVE refresh token by token value (for normal refresh flow)
        /// </summary>
        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Include(rt => rt.User) // Include user for token generation
                .FirstOrDefaultAsync(
                    rt => rt.Token == token &&
                          !rt.IsDeleted &&
                          !rt.IsRevoked &&
                          rt.ExpiryDate > DateTime.UtcNow,
                    cancellationToken);
        }

        /// <summary>
        /// 🔒 SECURITY: Gets a REVOKED token to detect replay attacks
        /// </summary>
        public async Task<RefreshToken?> GetRevokedTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(
                    rt => rt.Token == token &&
                          !rt.IsDeleted &&
                          rt.IsRevoked,
                    cancellationToken);
        }

        /// <summary>
        /// 🔒 SECURITY: Gets all tokens in a token family for breach response
        /// </summary>
        public async Task<List<RefreshToken>> GetTokenFamilyAsync(
            string tokenFamily,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(rt => rt.TokenFamily == tokenFamily && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all refresh tokens for a user (including revoked)
        /// </summary>
        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all ACTIVE refresh tokens for a user
        /// </summary>
        public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 🔒 SECURITY: Gets all active tokens for a user (used for session limit checks)
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new refresh token
        /// </summary>
        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            await _context.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        }

        /// <summary>
        /// Updates an existing refresh token
        /// </summary>
        public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            _context.Set<RefreshToken>().Update(refreshToken);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Deletes expired tokens (only tokens older than retention period)
        /// </summary>
        public async Task DeleteExpiredTokensAsync(
            int retentionDays = 30,
            CancellationToken cancellationToken = default)
        {
            // Only delete tokens that expired more than X days ago (for audit trail)
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var expiredTokens = await _context.Set<RefreshToken>()
                .Where(rt => !rt.IsDeleted &&
                            (rt.ExpiryDate < cutoffDate ||
                             (rt.IsRevoked && rt.RevokedAt < cutoffDate)))
                .ToListAsync(cancellationToken);

            if (expiredTokens.Any())
            {
                _context.Set<RefreshToken>().RemoveRange(expiredTokens);
            }
        }

        /// <summary>
        /// 🔒 SECURITY: Revokes all tokens for a user (logout all devices)
        /// </summary>
        public async Task RevokeAllTokensForUserAsync(
            Guid userId,
            string revokedByIp,
            string reason = "Logout all devices",
            CancellationToken cancellationToken = default)
        {
            var activeTokens = await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke(revokedByIp, reason);
            }
        }

        /// <summary>
        /// 🔒 SECURITY: Revokes all tokens in a family (for security breach)
        /// </summary>
        public async Task RevokeTokenFamilyAsync(
            string tokenFamily,
            string revokedByIp,
            string reason = "Token family compromised",
            CancellationToken cancellationToken = default)
        {
            var familyTokens = await _context.Set<RefreshToken>()
                .Where(rt => rt.TokenFamily == tokenFamily &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in familyTokens)
            {
                token.Revoke(revokedByIp, reason);
            }
        }

        /// <summary>
        /// Gets token statistics for monitoring
        /// </summary>
        public async Task<RefreshTokenStatisticsDto> GetStatisticsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var tokens = await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && !rt.IsDeleted)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            return new RefreshTokenStatisticsDto
            {
                TotalTokens = tokens.Count,
                ActiveTokens = tokens.Count(t => !t.IsRevoked && t.ExpiryDate > now),
                RevokedTokens = tokens.Count(t => t.IsRevoked),
                ExpiredTokens = tokens.Count(t => t.ExpiryDate <= now && !t.IsRevoked),
                OldestActiveToken = tokens
                    .Where(t => !t.IsRevoked && t.ExpiryDate > now)
                    .OrderBy(t => t.CreatedAt)
                    .FirstOrDefault()?.CreatedAt,
                NewestActiveToken = tokens
                    .Where(t => !t.IsRevoked && t.ExpiryDate > now)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault()?.CreatedAt
            };
        }
    }   
}
