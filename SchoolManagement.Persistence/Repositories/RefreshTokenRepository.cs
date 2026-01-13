// Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs - COMPILATION FIXED
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

        #region BASIC CRUD (TENANT-SCOPED)

        /// <summary>
        /// Get by ID with tenant validation
        /// </summary>
        public async Task<RefreshToken?> GetByIdAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.Id == id && !rt.IsDeleted && rt.TenantId == tenantId);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Add with tenant enforcement
        /// </summary>
        public async Task AddAsync(RefreshToken token, Guid tenantId, Guid schoolId, CancellationToken ct = default)
        {
            //if (token == null) throw new ArgumentNullException(nameof(token));

            //token.TenantId = tenantId;
            //token.SchoolId = schoolId;

            await _context.Set<RefreshToken>().AddAsync(token, ct);
        }

        /// <summary>
        /// Update with tenant validation
        /// </summary>
        public Task UpdateAsync(RefreshToken token, Guid tenantId, Guid? schoolId = null, CancellationToken ct = default)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            if (token.TenantId != tenantId || (schoolId.HasValue && token.SchoolId != schoolId.Value))
                throw new InvalidOperationException("Tenant/School mismatch");

            var entry = _context.Entry(token);
            if (entry.State == EntityState.Detached)
                _context.Set<RefreshToken>().Update(token);

            return Task.CompletedTask;
        }

        #endregion

        #region TOKEN LOOKUP (SECURITY)

        /// <summary>
        /// 🔑 ACTIVE refresh token (login/refresh)
        /// </summary>
        public async Task<RefreshToken?> GetByTokenAsync(string token, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .Where(rt => rt.Token == token &&
                            !rt.IsDeleted &&
                            rt.TenantId == tenantId &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// 🔒 Replay attack detection
        /// </summary>
        public async Task<RefreshToken?> GetRevokedTokenAsync(string token, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.Token == token &&
                            !rt.IsDeleted &&
                            rt.TenantId == tenantId &&
                            rt.IsRevoked);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// 🔒 Token family lookup (breach response)
        /// </summary>
        public async Task<List<RefreshToken>> GetTokenFamilyAsync(Guid tenantId, string tokenFamily,
            Guid? schoolId = null, CancellationToken ct = default)
        {
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.TokenFamily == tokenFamily &&
                            !rt.IsDeleted &&
                            rt.TenantId == tenantId);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            // ✅ FIXED: Explicit OrderByDescending variable
            var orderedQuery = query.OrderByDescending(rt => rt.CreatedAt);
            return await orderedQuery.ToListAsync(ct);
        }

        /// <summary>
        /// All user tokens (tenant-scoped)
        /// </summary>
        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, Guid tenantId,
            Guid? schoolId = null, CancellationToken ct = default)
        {
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            !rt.IsDeleted &&
                            rt.TenantId == tenantId);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            // ✅ FIXED: Explicit variable for OrderByDescending
            var orderedQuery = query.OrderByDescending(rt => rt.CreatedAt);
            return await orderedQuery.ToListAsync(ct);
        }

        /// <summary>
        /// Active tokens only (session count)
        /// </summary>
        public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, Guid tenantId,
            Guid? schoolId = null, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            rt.TenantId == tenantId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            // ✅ FIXED: Explicit OrderByDescending
            var orderedQuery = query.OrderByDescending(rt => rt.CreatedAt);
            return await orderedQuery.ToListAsync(ct);
        }

        #endregion

        #region BULK OPERATIONS

        public async Task DeleteExpiredTokensAsync(Guid tenantId, int retentionDays = 30,
            Guid? schoolId = null, CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.TenantId == tenantId &&
                            !rt.IsDeleted &&
                            (rt.ExpiryDate < cutoff ||
                             (rt.IsRevoked && rt.RevokedAt < cutoff)));

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            var expired = await query.ToListAsync(ct);
            if (expired.Any())
                _context.Set<RefreshToken>().RemoveRange(expired);
        }

        public async Task RevokeAllTokensForUserAsync(Guid userId, Guid tenantId, string revokedByIp,
            string reason = "Logout all devices", Guid? schoolId = null, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            rt.TenantId == tenantId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            var activeTokens = await query.ToListAsync(ct);
            foreach (var token in activeTokens)
                token.Revoke(revokedByIp, reason);
        }

        public async Task RevokeTokenFamilyAsync(Guid tenantId, string tokenFamily, string revokedByIp,
            string reason = "Token family compromised", Guid? schoolId = null, CancellationToken ct = default)
        {
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.TokenFamily == tokenFamily &&
                            rt.TenantId == tenantId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            var familyTokens = await query.ToListAsync(ct);
            foreach (var token in familyTokens)
                token.Revoke(revokedByIp, reason);
        }

        #endregion

        #region MONITORING & ADMIN

        public async Task<RefreshTokenStatisticsDto> GetStatisticsAsync(Guid userId, Guid tenantId,
            CancellationToken ct = default)
        {
            var tokens = await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && rt.TenantId == tenantId && !rt.IsDeleted)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            return new RefreshTokenStatisticsDto
            {
                TotalTokens = tokens.Count,
                ActiveTokens = tokens.Count(t => !t.IsRevoked && t.ExpiryDate > now),
                RevokedTokens = tokens.Count(t => t.IsRevoked),
                ExpiredTokens = tokens.Count(t => t.ExpiryDate <= now && !t.IsRevoked)
            };
        }

        public async Task<int> GetActiveSessionCountAsync(Guid userId, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            rt.TenantId == tenantId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            return await query.CountAsync(ct);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllTenantTokensAsync(Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.TenantId == tenantId && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt);

            if (schoolId.HasValue)
                query = (IOrderedQueryable<RefreshToken>)query.Where(rt => rt.SchoolId == schoolId.Value);

            return await query.Take(1000).ToListAsync(ct);
        }

        public async Task RevokeAllTenantTokensAsync(Guid tenantId, string adminIp, string reason,
            Guid? schoolId = null, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Set<RefreshToken>()
                .Where(rt => rt.TenantId == tenantId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now);

            if (schoolId.HasValue)
                query = query.Where(rt => rt.SchoolId == schoolId.Value);

            var activeTokens = await query.ToListAsync(ct);
            foreach (var token in activeTokens)
                token.Revoke(adminIp, reason);
        }

        #endregion
    }
}