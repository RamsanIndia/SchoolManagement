// Infrastructure/Persistence/Repositories/UserRepository.cs - MULTI-TENANT IMPLEMENTATION
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using SchoolManagement.Persistence;
using SchoolManagement.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(SchoolManagementDbContext context) : base(context) { }

        #region QUERY - Single User (TENANT ISOLATED)

        /// <summary>
        /// Get tenant-aware queryable with global filters
        /// </summary>
        public IQueryable<User> GetQueryable(Guid? schoolId = null)
        {
            var query = _context.Users.IgnoreQueryFilters().Where(u => !u.IsDeleted);

            if (schoolId.HasValue)
                query = query.Where(u => u.SchoolId == schoolId.Value);

            return query;
        }

        public async Task<User?> GetByIdAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId).Where(u => u.Id == id && u.TenantId == tenantId);
            return await query.AsNoTracking().FirstOrDefaultAsync(ct);
        }

        public async Task<User?> GetByIdWithRolesAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId)
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role);

            return await query.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(ct);
        }

        public async Task<User?> GetByIdWithTokensAsync(Guid id, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId)
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .Include(u => u.RefreshTokens.Where(rt => !rt.IsDeleted));

            return await query.AsNoTracking().FirstOrDefaultAsync(ct);
        }

        public async Task<User?> GetByIdWithSchoolAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        {
            var query = GetQueryable()
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .Include(u => u.School);

            return await query.FirstOrDefaultAsync(ct);
        }

        #endregion

        #region QUERY - Email/Username (TENANT CRITICAL)

        /// <summary>
        /// Login/registration - tenant isolated
        /// </summary>
        //public async Task<User?> GetByEmailAsync(string email, Guid tenantId, Guid? schoolId = null,
        //    CancellationToken ct = default)
        //{
        //    if (string.IsNullOrWhiteSpace(email)) return null;

        //    var emailVo = new Email(email.ToLower().Trim());
        //    var query = GetQueryable(schoolId)
        //        .Where(u => u.Email == emailVo && u.TenantId == tenantId)
        //        .Include(u => u.RefreshTokens.Where(rt => !rt.IsDeleted));

        //    return await query.AsNoTracking().FirstOrDefaultAsync(ct);
        //}

        public async Task<User?> GetByEmailAsync(string email, Guid tenantId,  CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var emailVo = new Email(email.ToLower().Trim());

            // ✅ CRITICAL FIX: Ignore tenant filter for login - user's tenant is discovered FROM the user record
            // Login is "tenant discovery" not "tenant validation"
            var query = _context.Users
                .IgnoreQueryFilters()  // Bypass global tenant filter
                .Where(u => u.Email == emailVo && u.IsActive && !u.IsDeleted)  // Only email + status checks
                .Include(u => u.RefreshTokens.Where(rt => !rt.IsDeleted));

            var user = await query.AsNoTracking().FirstOrDefaultAsync(ct);

            return user;
        }


        public async Task<User?> GetByEmailWithRolesAsync(string email, Guid tenantId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var emailVo = new Email(email.ToLower().Trim());
            var query = GetQueryable()
                .Where(u => u.Email == emailVo && u.TenantId == tenantId)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role);

            return await query.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Pessimistic lock for login (change-tracked)
        /// </summary>
        public async Task<User?> GetByEmailWithLockAsync(string email, Guid tenantId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var emailVo = new Email(email.ToLower().Trim());
            var query = GetQueryable()
                .Where(u => u.Email == emailVo && u.TenantId == tenantId)
                .Include(u => u.RefreshTokens.Where(rt => !rt.IsDeleted));

            // NO AsNoTracking - need change tracking for login updates
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<User?> GetByUsernameAsync(string username, Guid tenantId,  CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var normalized = username.Trim();
            var query = GetQueryable() 
                .Where(u => u.Username == normalized && u.TenantId == tenantId);

            return await query.AsNoTracking().FirstOrDefaultAsync(ct);
        }



        public async Task<User?> GetByEmailWithTokensAsync(string email, Guid tenantId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var emailVo = new Email(email.ToLower().Trim());
            var query = GetQueryable()
                .Where(u => u.Email == emailVo && u.TenantId == tenantId)
                .Include(u => u.RefreshTokens.Where(rt => !rt.IsDeleted));

            return await query.FirstOrDefaultAsync(ct);
        }

        #endregion

        #region QUERY - Collections

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
            Guid tenantId, Guid? schoolId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId)
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.Username);

            var total = await query.CountAsync(ct);
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return (users, total);
        }

        public async Task<IEnumerable<User>> GetByUserTypeAsync(UserType userType, Guid tenantId,
            Guid? schoolId = null, CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId)
                .Where(u => u.TenantId == tenantId && u.UserType == userType);

            return await query.AsNoTracking().OrderBy(u => u.Username).ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync(Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId)
                .Where(u => u.TenantId == tenantId && u.IsActive);

            return await query.AsNoTracking().OrderBy(u => u.Username).ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> SearchAsync(Guid tenantId, string searchTerm, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return [];

            var query = GetQueryable(schoolId)
                .Where(u => u.TenantId == tenantId &&
                    (EF.Functions.Like(u.Username, $"%{searchTerm}%") ||
                     EF.Functions.Like(u.Email.Value, $"%{searchTerm}%")));

            return await query.AsNoTracking().OrderBy(u => u.Username).Take(50).ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetByIdsAsync(Guid tenantId, IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            if (!ids?.Any() == true) return [];

            var query = GetQueryable()
                .Where(u => u.TenantId == tenantId && ids.Contains(u.Id));

            return await query.AsNoTracking().ToListAsync(ct);
        }

        #endregion

        #region ADMIN/REPORTS

        public async Task<IEnumerable<User>> GetUsersWithExpiringRolesAsync(Guid tenantId, DateTime expiryDate,
            CancellationToken ct = default)
        {
            var query = GetQueryable()
                .Where(u => u.TenantId == tenantId &&
                    u.UserRoles.Any(ur => ur.IsActive && ur.ExpiresAt <= expiryDate))
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role);

            return await query.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetLockedOutUsersAsync(Guid tenantId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = GetQueryable()
                .Where(u => u.TenantId == tenantId &&
                    u.LockedUntil > now);

            return await query.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetUnverifiedEmailUsersAsync(Guid tenantId, CancellationToken ct = default)
        {
            var query = GetQueryable()
                .Where(u => u.TenantId == tenantId && u.IsActive && !u.EmailVerified);

            return await query.AsNoTracking().OrderBy(u => u.CreatedAt).ToListAsync(ct);
        }

        #endregion

        #region COMMANDS

        public async Task AddAsync(User user, Guid tenantId, Guid schoolId, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Enforce tenant context
            if (user.TenantId != tenantId || user.SchoolId != schoolId)
                throw new InvalidOperationException("Tenant/School mismatch");

            await _context.Users.AddAsync(user, ct);
        }

        public Task UpdateAsync(User user, Guid tenantId, Guid? schoolId = null, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Validate tenant context
            if (user.TenantId != tenantId ||
                (schoolId.HasValue && user.SchoolId != schoolId.Value))
                throw new InvalidOperationException("Tenant/School mismatch");

            var entry = _context.Entry(user);
            if (entry.State == EntityState.Detached)
                _context.Users.Update(user);

            return Task.CompletedTask;
        }

        public async Task<User> GetByRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .Include(u => u.School)
                    .ThenInclude(s => s.Tenant)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(rt => rt.Token == refreshToken && rt.IsActive),
                    cancellationToken);
        }

        #endregion

        #region VALIDATION

        public async Task<bool> EmailExistsAsync(string email, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var emailVo = new Email(email.ToLower().Trim());
            var query = GetQueryable(schoolId)
                .Where(u => u.TenantId == tenantId && u.Email == emailVo);

            return await query.AnyAsync(ct);
        }

        public async Task<bool> UsernameExistsAsync(string username, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            var normalized = username.Trim();
            var query = GetQueryable(schoolId)
                .Where(u => u.TenantId == tenantId && u.Username == normalized);

            return await query.AnyAsync(ct);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid tenantId, Guid? schoolId = null,
            CancellationToken ct = default)
        {
            var query = GetQueryable(schoolId)
                .Where(u => u.Id == userId && u.TenantId == tenantId);

            return await query.AnyAsync(ct);
        }

        #endregion
    }
}
