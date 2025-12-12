// Application/Interfaces/IUserService.cs
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IUserService
    {
        // Query methods
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetByUserTypeAsync(UserType userType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

        // Command methods
        Task<User> CreateAsync(User user, string password, string createdBy, string createdIp, CancellationToken cancellationToken = default);
        Task<User> UpdateAsync(User user, string updatedBy, string updatedIp, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);

        // Password methods
        Task<bool> ValidatePasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(Guid userId, string newPassword, string updatedBy, CancellationToken cancellationToken = default);

        // Role methods
        Task AssignRoleAsync(Guid userId, Guid roleId, string assignedBy, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
        Task RevokeRoleAsync(Guid userId, Guid roleId, string revokedBy, CancellationToken cancellationToken = default);

        // User status methods
        Task<User> ActivateUserAsync(Guid userId, string activatedBy, CancellationToken cancellationToken = default);
        Task<User> DeactivateUserAsync(Guid userId, string deactivatedBy, string reason = null, CancellationToken cancellationToken = default);
        Task<User> VerifyEmailAsync(Guid userId, string verifiedBy, CancellationToken cancellationToken = default);
        Task<User> VerifyPhoneAsync(Guid userId, string verifiedBy, CancellationToken cancellationToken = default);
        Task<User> UnlockUserAsync(Guid userId, string unlockedBy, CancellationToken cancellationToken = default);
    }
}
