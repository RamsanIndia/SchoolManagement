// Application/Interfaces/IRefreshTokenRepository.cs - MULTI-TENANT VERSION
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    /// <summary>
    /// Refresh token repository with tenant isolation
    /// All operations scoped to TenantId + SchoolId
    /// </summary>
    public interface IRefreshTokenRepository
    {
        #region BASIC CRUD (TENANT-SCOPED)

        /// <summary>
        /// Get token by ID within tenant context
        /// </summary>
        Task<RefreshToken?> GetByIdAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Add token (sets TenantId/SchoolId)
        /// </summary>
        Task AddAsync(RefreshToken token, Guid tenantId, Guid schoolId, CancellationToken ct = default);

        /// <summary>
        /// Update token (validates tenant)
        /// </summary>
        Task UpdateAsync(RefreshToken token, Guid tenantId, Guid? schoolId = null, CancellationToken ct = default);

        #endregion

        #region TOKEN LOOKUP (SECURITY CRITICAL)

        /// <summary>
        /// 🔑 Get ACTIVE token by value (login/refresh)
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string token, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// 🔒 Detect replay attacks (revoked tokens)
        /// </summary>
        Task<RefreshToken?> GetRevokedTokenAsync(string token, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// 🔒 Get all tokens in family (security breach)
        /// </summary>
        Task<List<RefreshToken>> GetTokenFamilyAsync(Guid tenantId, string tokenFamily, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get all tokens for user (tenant-isolated)
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get ACTIVE tokens for user (session count)
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        #endregion

        #region BULK OPERATIONS (TENANT-SCOPED)

        /// <summary>
        /// 🔄 Cleanup expired tokens (tenant-wide)
        /// </summary>
        Task DeleteExpiredTokensAsync(Guid tenantId, int retentionDays = 30, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// 🔒 Logout all devices (tenant-validated)
        /// </summary>
        Task RevokeAllTokensForUserAsync(Guid userId, Guid tenantId, string revokedByIp,
            string reason = "Logout all devices", Guid? schoolId = null, CancellationToken ct = default);

        /// <summary>
        /// 🔒 Revoke token family (breach response)
        /// </summary>
        Task RevokeTokenFamilyAsync(Guid tenantId, string tokenFamily, string revokedByIp,
            string reason = "Token family compromised", Guid? schoolId = null, CancellationToken ct = default);

        #endregion

        #region MONITORING

        /// <summary>
        /// Token statistics per user/tenant
        /// </summary>
        Task<RefreshTokenStatisticsDto> GetStatisticsAsync(Guid userId, Guid tenantId,
            CancellationToken ct = default);

        /// <summary>
        /// Active session count (rate limiting)
        /// </summary>
        Task<int> GetActiveSessionCountAsync(Guid userId, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        #endregion

        /// <summary>
        /// Get all tokens in tenant (admin dashboard)
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetAllTenantTokensAsync(Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Revoke all tokens in tenant (emergency)
        /// </summary>
        Task RevokeAllTenantTokensAsync(Guid tenantId, string adminIp, string reason,
            Guid? schoolId = null, CancellationToken ct = default);
    }
}

