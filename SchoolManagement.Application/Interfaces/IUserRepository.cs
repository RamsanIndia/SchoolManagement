// Application/Interfaces/IUserRepository.cs - MULTI-TENANT REFACTORED
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    /// <summary>
    /// User repository with multi-tenant support (TenantId + SchoolId filtering)
    /// Inherits IRepository for CRUD operations
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        #region QUERY - Single User (Tenant/School Scoped)

        /// <summary>
        /// Get tenant-aware queryable with auto SchoolId filter
        /// </summary>
        IQueryable<User> GetQueryable(Guid? schoolId = null);

        /// <summary>
        /// Get user by ID within tenant/school context
        /// </summary>
        Task<User?> GetByIdAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get by ID with roles (tenant-isolated)
        /// </summary>
        Task<User?> GetByIdWithRolesAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get by ID with tokens (security operations)
        /// </summary>
        Task<User?> GetByIdWithTokensAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get by ID with school details
        /// </summary>
        Task<User?> GetByIdWithSchoolAsync(Guid id, Guid tenantId, CancellationToken ct = default);

        #endregion

        #region QUERY - Email/Username (TENANT ISOLATED - CRITICAL)

        /// <summary>
        /// Get by email WITHIN TENANT (login/registration)
        /// </summary>
        Task<User?> GetByEmailAsync(string email, Guid tenantId,
            CancellationToken ct = default);

        /// <summary>
        /// Get by email with roles (authorization)
        /// </summary>
        Task<User?> GetByEmailWithRolesAsync(string email, Guid tenantId,
            CancellationToken ct = default);

        /// <summary>
        /// Get by email with pessimistic lock (login - prevents race conditions)
        /// </summary>
        Task<User?> GetByEmailWithLockAsync(string email, Guid tenantId,
            CancellationToken ct = default);

        /// <summary>
        /// Get by username within tenant
        /// </summary>
        Task<User?> GetByUsernameAsync(string username, Guid tenantId,
            CancellationToken ct = default);

        /// <summary>
        /// Get by email with tokens (refresh token validation)
        /// </summary>
        Task<User?> GetByEmailWithTokensAsync(string email, Guid tenantId,
            CancellationToken ct = default);

        #endregion

        #region QUERY - Collections (TENANT-SCOPED)

        /// <summary>
        /// Get all users in tenant/school (paginated)
        /// </summary>
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
            Guid tenantId, Guid? schoolId, int pageNumber, int pageSize,
            CancellationToken ct = default);

        /// <summary>
        /// Get users by type within tenant
        /// </summary>
        Task<IEnumerable<User>> GetByUserTypeAsync(UserType userType, Guid tenantId,
            Guid? schoolId = null, CancellationToken ct = default);

        /// <summary>
        /// Get active users (tenant-scoped)
        /// </summary>
        Task<IEnumerable<User>> GetActiveUsersAsync(Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Search users (tenant + school filtered)
        /// </summary>
        Task<IEnumerable<User>> SearchAsync(Guid tenantId, string searchTerm, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get users by IDs (all must belong to same tenant)
        /// </summary>
        Task<IEnumerable<User>> GetByIdsAsync(Guid tenantId, IEnumerable<Guid> ids,
            CancellationToken ct = default);

        #endregion

        #region QUERY - Admin/Reports (TENANT-SCOPED)

        /// <summary>
        /// Get users with expiring roles
        /// </summary>
        Task<IEnumerable<User>> GetUsersWithExpiringRolesAsync(Guid tenantId, DateTime expiryDate,
            CancellationToken ct = default);

        /// <summary>
        /// Get locked out users
        /// </summary>
        Task<IEnumerable<User>> GetLockedOutUsersAsync(Guid tenantId, CancellationToken ct = default);

        /// <summary>
        /// Get unverified email users
        /// </summary>
        Task<IEnumerable<User>> GetUnverifiedEmailUsersAsync(Guid tenantId, CancellationToken ct = default);

        #endregion

        #region COMMANDS (TENANT-AWARE)

        /// <summary>
        /// Add user (sets TenantId/SchoolId automatically)
        /// </summary>
        Task AddAsync(User user, Guid tenantId, Guid schoolId, CancellationToken ct = default);

        /// <summary>
        /// Update user (validates tenant/school match)
        /// </summary>
        Task UpdateAsync(User user, Guid tenantId, Guid? schoolId = null, CancellationToken ct = default);

        /// <summary>
        /// Gets user by refresh token (includes school and tenant navigation)
        /// </summary>
        /// <param name="refreshToken">The refresh token string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with school and tenant loaded, or null if not found</returns>
        Task<User> GetByRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);
    
        #endregion

        #region VALIDATION (TENANT ISOLATED)

        /// <summary>
        /// Check email exists within tenant
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Check username exists within tenant
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Check user exists by ID within tenant
        /// </summary>
        Task<bool> ExistsAsync(Guid userId, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default);

        #endregion
    }
}
