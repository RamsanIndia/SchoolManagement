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
                .AsSingleQuery()  // ADDED: Force single query to bypass split query cache
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTrackingWithIdentityResolution()
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
        /// Get user by email for authentication
        /// </summary>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.ToLower().Trim();
            var emailValueObject = new Email(normalizedEmail);

            return await _context.Users
                .Include(u => u.RefreshTokens)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == emailValueObject && !u.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Get user by email with roles
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
        /// ✅ FIXED: Get user by email with pessimistic lock for login operations
        /// CRITICAL FIXES:
        /// 1. Create Email value object for proper comparison
        /// 2. Include RefreshTokens collection for tracking
        /// 3. Do NOT use AsNoTracking() - we need change tracking for updates
        /// </summary>
        public async Task<User?> GetByEmailWithLockAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.ToLower().Trim();

            // ✅ FIX #1: Create Email value object for proper comparison
            var emailValueObject = new Email(normalizedEmail);

            // ✅ FIX #2: Include RefreshTokens AND remove AsNoTracking()
            var user = await _context.Users
                .Include(u => u.RefreshTokens)  // MUST include for collection tracking
                                                // NO AsNoTracking() here - we need change tracking for login operations!
                .FirstOrDefaultAsync(u => u.Email == emailValueObject && !u.IsDeleted, cancellationToken);

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

        /// <summary>
        /// ✅ SIMPLIFIED: Update user - EF tracks changes automatically
        /// </summary>
        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // EF Core automatically tracks changes to entities loaded with Include()
            // No need to call Update() explicitly if entity is already tracked
            var entry = _context.Entry(user);

            if (entry.State == EntityState.Detached)
            {
                _context.Users.Update(user);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Validation Methods

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalizedEmail = email.ToLower().Trim();
            var emailValueObject = new Email(normalizedEmail);

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == emailValueObject && !u.IsDeleted, cancellationToken);
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

        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.Username)
                .ToListAsync(cancellationToken);
        }

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

        public async Task<IEnumerable<User>> GetUnverifiedEmailUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.IsActive && !u.EmailVerified)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        //public async Task<User> GetByEmailWithTokensAsync(string email, CancellationToken cancellationToken = default)
        //{
        //    return await _context.Users
        //        .Include(u => u.RefreshTokens) // Load all refresh tokens
        //        .FirstOrDefaultAsync(u => u.Email.Value == email && !u.IsDeleted, cancellationToken);
        //}

        public async Task<User?> GetByEmailWithTokensAsync(string email, CancellationToken cancellationToken = default)
        {
            var emailVo = new Email(email);

            return await _context.Users
                .Include(u => u.RefreshTokens.Where(rt => !rt.IsDeleted))
                .FirstOrDefaultAsync(
                    u => u.Email == emailVo.Value,
                    cancellationToken);
        }


        #endregion
    }
}