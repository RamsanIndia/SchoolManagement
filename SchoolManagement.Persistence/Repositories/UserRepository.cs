// Infrastructure/Persistence/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SchoolManagementDbContext _context;

        public UserRepository(SchoolManagementDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Query Methods

        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        }

        public async Task<User> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        }

        public async Task<User> GetByIdWithTokensAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Get user by email for authentication - uses AsNoTracking for concurrency safety
        /// </summary>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.ToLower().Trim();
            var emailValueObject = new Email(normalizedEmail);

            // CRITICAL FIX: Use AsNoTracking() to prevent caching issues
            // Include RefreshTokens for login operations but not Roles (not needed for auth)
            return await _context.Users
                .Include(u => u.RefreshTokens)  // Need tokens for login
                .AsNoTracking()  // ← CRITICAL: Prevents entity caching
                .FirstOrDefaultAsync(u => u.Email == emailValueObject && !u.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Get user by email with roles - for operations that need role information
        /// </summary>
        public async Task<User> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.ToLower().Trim();
            var emailValueObject = new Email(normalizedEmail);

            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == emailValueObject && !u.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Get user by email with pessimistic lock - prevents concurrent modifications
        /// Use this for login operations to avoid concurrency conflicts
        /// </summary>
        public async Task<User> GetByEmailWithLockAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.ToLower().Trim();

            // Use SQL Server row-level locking to prevent concurrent modifications
            // FromSqlRaw MUST come before Include
            var user = await _context.Users
                .FromSqlRaw(@"
                    SELECT * 
                    FROM Users WITH (UPDLOCK, ROWLOCK)
                    WHERE Email = {0} AND IsDeleted = 0",
                    normalizedEmail)
                .Include(u => u.RefreshTokens)  // Include after FromSqlRaw
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var normalizedUsername = username.Trim();

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Username == normalizedUsername && !u.IsDeleted,
                    cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Username)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetByUserTypeAsync(
            UserType userType,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == userType && !u.IsDeleted)
                .OrderBy(u => u.Username)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region Command Methods

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user, cancellationToken);
        }

        //public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException(nameof(user));

        //    // CRITICAL: Detach any existing tracked entity before updating
        //    var trackedEntity = _context.ChangeTracker.Entries<User>()
        //        .FirstOrDefault(e => e.Entity.Id == user.Id);

        //    if (trackedEntity != null)
        //    {
        //        trackedEntity.State = EntityState.Detached;
        //    }

        //    // Also detach related RefreshTokens to avoid tracking conflicts
        //    var trackedTokens = _context.ChangeTracker.Entries<RefreshToken>()
        //        .Where(e => e.Entity.UserId == user.Id)
        //        .ToList();

        //    foreach (var token in trackedTokens)
        //    {
        //        token.State = EntityState.Detached;
        //    }

        //    _context.Users.Update(user);
        //    return Task.CompletedTask;
        //}

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // ✅ SIMPLIFIED - Let EF handle tracking naturally
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        #endregion

        #region Validation Methods

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalizedEmail = email.ToLower().Trim();

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(
                    u => EF.Property<string>(u, "Email") == normalizedEmail && !u.IsDeleted,
                    cancellationToken);
        }

        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var normalizedUsername = username.Trim();

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.Username == normalizedUsername && !u.IsDeleted,
                    cancellationToken);
        }

        #endregion

        #region Additional Query Methods

        /// <summary>
        /// Get users with pagination
        /// </summary>
        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }

        /// <summary>
        /// Get active users only
        /// </summary>
        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.Username)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get users by multiple IDs (for batch operations)
        /// </summary>
        public async Task<IEnumerable<User>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<User>();

            return await _context.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id) && !u.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Search users by name or email
        /// </summary>
        public async Task<IEnumerable<User>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<User>();

            var normalizedSearchTerm = searchTerm.ToLower().Trim();

            return await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted &&
                    (u.Username.ToLower().Contains(normalizedSearchTerm) ||
                     EF.Property<string>(u, "Email").Contains(normalizedSearchTerm) ||
                     EF.Property<string>(u.FullName, "FirstName").ToLower().Contains(normalizedSearchTerm) ||
                     EF.Property<string>(u.FullName, "LastName").ToLower().Contains(normalizedSearchTerm)))
                .OrderBy(u => u.Username)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get users with expired or expiring roles
        /// </summary>
        public async Task<IEnumerable<User>> GetUsersWithExpiringRolesAsync(
            DateTime expiryDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted &&
                    u.UserRoles.Any(ur =>
                        ur.IsActive &&
                        ur.ExpiresAt.HasValue &&
                        ur.ExpiresAt.Value <= expiryDate))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get locked out users
        /// </summary>
        public async Task<IEnumerable<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted &&
                    u.LockedUntil.HasValue &&
                    u.LockedUntil.Value > now)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get users pending email verification
        /// </summary>
        public async Task<IEnumerable<User>> GetUnverifiedEmailUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.IsActive && !u.EmailVerified)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}