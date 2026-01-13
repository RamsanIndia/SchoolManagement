// Application/Services/UserService.cs
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly ITenantService _tenantService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger,
            ITenantService tenantService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantService = tenantService;
        }

        #region Query Methods
         
        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByEmailAsync(email, _tenantService.TenantId, cancellationToken);
        }

        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByUsernameAsync(username, _tenantService.TenantId,  cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetByUserTypeAsync(
            UserType userType,
            CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByUserTypeAsync(userType, _tenantService.TenantId,_tenantService.SchoolId, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId, _tenantService.TenantId, _tenantService.SchoolId, cancellationToken);
            if (user == null)
                throw new ArgumentException($"User not found with ID: {userId}");

            return user.UserRoles
                .Where(ur => ur.IsActive && !ur.IsExpired())
                .Select(ur => ur.Role)
                .Where(r => r != null && r.IsActive)
                .ToList();
        }

        #endregion

        #region Command Methods

        public async Task<User> CreateAsync(
            User user,
            string password,
            string createdBy,
            string createdIp,
            CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                // Validate user doesn't already exist
                var existingByEmail = await _userRepository.GetByEmailAsync(user.Email.Value, _tenantService.TenantId, cancellationToken);
                if (existingByEmail != null)
                    throw new InvalidOperationException($"User with email '{user.Email.Value}' already exists");

                var existingByUsername = await _userRepository.GetByUsernameAsync(user.Username, _tenantService.TenantId, cancellationToken);
                if (existingByUsername != null)
                    throw new InvalidOperationException($"User with username '{user.Username}' already exists");

                // Hash password
                var hashedPassword = _passwordService.HashPassword(password);
                user.UpdatePassword(hashedPassword, createdBy);

                // Set audit fields (will also be set by DbContext, but explicit is better)
                user.SetCreated(createdBy, createdIp);

                await _userRepository.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "User created successfully: {UserId}, Username: {Username}, Email: {Email}",
                    user.Id,
                    user.Username,
                    user.Email.Value);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user.Username);
                throw;
            }
        }

        public async Task<User> UpdateAsync(
            User user,
            string updatedBy,
            string updatedIp,
            CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                user.SetUpdated(updatedBy, updatedIp);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User updated successfully: {UserId}", user.Id);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            string deletedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(id, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("Delete attempted for non-existent user: {UserId}", id);
                    return false;
                }

                // Soft delete
                user.Deactivate(deletedBy, "Deleted by user");
                user.MarkAsDeleted();

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User soft deleted: {UserId} by {DeletedBy}", id, deletedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }

        #endregion

        #region Password Methods

        public async Task<bool> ValidatePasswordAsync(
            Guid userId,
            string password,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Password validation attempted for non-existent user: {UserId}", userId);
                return false;
            }

            return _passwordService.VerifyPassword(password, user.PasswordHash);
        }

        public async Task ChangePasswordAsync(
            Guid userId,
            string newPassword,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be null or empty", nameof(newPassword));

            try
            {
                var user = await GetByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                var hashedPassword = _passwordService.HashPassword(newPassword);
                user.UpdatePassword(hashedPassword, updatedBy);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password changed for user: {UserId} by {UpdatedBy}", userId, updatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Role Methods

        public async Task AssignRoleAsync(
            Guid userId,
            Guid roleId,
            string assignedBy,
            DateTime? expiresAt = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdWithRolesAsync(userId, _tenantService.TenantId, _tenantService.SchoolId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
                if (role == null)
                    throw new ArgumentException($"Role not found with ID: {roleId}");

                var existingRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId && ur.IsActive);
                if (existingRole != null)
                {
                    if (expiresAt.HasValue)
                    {
                        existingRole.ExtendRole(expiresAt.Value);
                        _logger.LogInformation(
                            "Extended role {RoleId} ({RoleName}) for user {UserId} until {ExpiresAt}",
                            roleId,
                            role.Name,
                            userId,
                            expiresAt.Value);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Role {RoleId} ({RoleName}) already assigned to user {UserId}",
                            roleId,
                            role.Name,
                            userId);
                    }
                }
                else
                {
                    var newUserRole = Domain.Entities.UserRole.Create(userId, roleId, assignedBy, expiresAt);
                    user.UserRoles.Add(newUserRole);
                    _logger.LogInformation(
                        "Assigned role {RoleId} ({RoleName}) to user {UserId}",
                        roleId,
                        role.Name,
                        userId);
                }

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
                throw;
            }
        }

        public async Task RevokeRoleAsync(
            Guid userId,
            Guid roleId,
            string revokedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdWithRolesAsync(userId, _tenantService.TenantId, _tenantService.SchoolId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId && ur.IsActive);
                if (userRole != null)
                {
                    userRole.DeactivateRole(revokedBy);
                    await _userRepository.UpdateAsync(user, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Revoked role {RoleId} from user {UserId} by {RevokedBy}",
                        roleId,
                        userId,
                        revokedBy);
                }
                else
                {
                    _logger.LogWarning(
                        "Attempted to revoke role {RoleId} from user {UserId}, but role was not assigned or already revoked",
                        roleId,
                        userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking role {RoleId} from user {UserId}", roleId, userId);
                throw;
            }
        }

        #endregion

        #region User Status Methods

        public async Task<User> ActivateUserAsync(
            Guid userId,
            string activatedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                user.Activate(activatedBy);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User activated: {UserId} by {ActivatedBy}", userId, activatedBy);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                throw;
            }
        }

        public async Task<User> DeactivateUserAsync(
            Guid userId,
            string deactivatedBy,
            string reason = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                user.Deactivate(deactivatedBy, reason);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "User deactivated: {UserId} by {DeactivatedBy}. Reason: {Reason}",
                    userId,
                    deactivatedBy,
                    reason ?? "Not specified");

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                throw;
            }
        }

        public async Task<User> VerifyEmailAsync(
            Guid userId,
            string verifiedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                user.VerifyEmail(verifiedBy);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Email verified for user: {UserId}", userId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<User> VerifyPhoneAsync(
            Guid userId,
            string verifiedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                user.VerifyPhone(verifiedBy);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Phone verified for user: {UserId}", userId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying phone for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<User> UnlockUserAsync(
            Guid userId,
            string unlockedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ArgumentException($"User not found with ID: {userId}");

                user.UnlockAccount(unlockedBy);

                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User unlocked: {UserId} by {UnlockedBy}", userId, unlockedBy);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
                throw;
            }
        }

        #endregion
    }
}
