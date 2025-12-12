using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        // Query Methods
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User> GetByIdWithTokensAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by email - uses AsNoTracking for concurrency safety
        /// </summary>
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by email with roles
        /// </summary>
        Task<User> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by email with pessimistic lock - prevents concurrent modifications
        /// Use this for login operations to avoid concurrency conflicts
        /// </summary>
        Task<User> GetByEmailWithLockAsync(string email, CancellationToken cancellationToken = default);

        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetByUserTypeAsync(UserType userType, CancellationToken cancellationToken = default);

        // Command Methods
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);

        // Validation Methods
        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

        // Additional Query Methods
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersWithExpiringRolesAsync(DateTime expiryDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUnverifiedEmailUsersAsync(CancellationToken cancellationToken = default);
    }
}