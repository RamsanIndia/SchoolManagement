// Domain/Entities/User.cs
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        // Value Objects for better domain modeling
        public string Username { get; private set; }
        public Email Email { get; private set; }
        public FullName FullName { get; private set; }
        public string PasswordHash { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }

        // Simple properties
        public bool IsActive { get; private set; }
        public bool EmailVerified { get; private set; }
        public bool PhoneVerified { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public int LoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }
        public UserType UserType { get; private set; }
        public Guid? StudentId { get; private set; }
        public Guid? EmployeeId { get; private set; }

        // Collection managed by aggregate root
        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        // Navigation Properties
        public virtual Student Student { get; private set; }
        public virtual Employee Employee { get; private set; }
        public virtual ICollection<UserRole> UserRoles { get; private set; }

        // Private constructor for EF Core
        private User() : base()
        {
            UserRoles = new List<UserRole>();
        }

        // Factory method for creating new users
        public static User Create(
            string username,
            Email email,
            FullName fullName,
            string passwordHash,
            UserType userType,
            string createdBy,
            string createdIp,
            string correlationId)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

            var user = new User
            {
                Username = username,
                Email = email ?? throw new ArgumentNullException(nameof(email)),
                FullName = fullName ?? throw new ArgumentNullException(nameof(fullName)),
                PasswordHash = passwordHash,
                UserType = userType,
                IsActive = true,
                EmailVerified = false,
                PhoneVerified = false,
                LoginAttempts = 0,
                UserRoles = new List<UserRole>()
            };

            user.SetCreated(createdBy, createdIp);

            // Raise domain event
            user.AddDomainEvent(new UserCreatedEvent(
                user.Id,
                user.Username,
                user.Email.Value,
                user.UserType));

            return user;
        }

        // Password management
        public void UpdatePassword(string newPasswordHash, string updatedBy, string updatedIp = "")
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            SetUpdated(updatedBy, updatedIp);

            AddDomainEvent(new UserPasswordChangedEvent(Id, Username));
        }

        // Profile management
        public void UpdateProfile(FullName fullName, Email email, string updatedBy, string updatedIp = "")
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));

            var emailChanged = !Email.Equals(email);
            Email = email ?? throw new ArgumentNullException(nameof(email));

            if (emailChanged)
            {
                EmailVerified = false;
                AddDomainEvent(new UserEmailChangedEvent(Id, Email.Value));
            }

            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserProfileUpdatedEvent(Id, FullName.FirstName, FullName.LastName));
        }

        public void UpdatePhoneNumber(PhoneNumber phoneNumber, string updatedBy, string updatedIp = "")
        {
            var phoneChanged = PhoneNumber == null || !PhoneNumber.Equals(phoneNumber);
            PhoneNumber = phoneNumber;

            if (phoneChanged)
            {
                PhoneVerified = false;
                AddDomainEvent(new UserPhoneNumberChangedEvent(Id, phoneNumber?.Value));
            }

            SetUpdated(updatedBy, updatedIp);
        }

        // Account status management
        public void Activate(string updatedBy, string updatedIp = "")
        {
            if (IsActive) return;

            IsActive = true;
            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserActivatedEvent(Id, Username));
        }

        public void Deactivate(string updatedBy, string updatedIp = "", string reason = null)
        {
            if (!IsActive) return;

            IsActive = false;
            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserDeactivatedEvent(Id, Username, reason));
        }

        // Email and phone verification
        public void VerifyEmail(string updatedBy, string updatedIp = "")
        {
            if (EmailVerified) return;

            EmailVerified = true;
            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserEmailVerifiedEvent(Id, Email.Value));
        }

        public void VerifyPhone(string updatedBy, string updatedIp = "")
        {
            if (PhoneVerified) return;

            PhoneVerified = true;
            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserPhoneVerifiedEvent(Id, PhoneNumber?.Value));
        }

        // User linking
        public void LinkToStudent(Guid studentId, string updatedBy, string updatedIp = "")
        {
            if (UserType != UserType.Student && UserType != UserType.Parent)
                throw new InvalidOperationException("Only student or parent users can be linked to students");

            StudentId = studentId;
            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserLinkedToStudentEvent(Id, studentId));
        }

        public void LinkToEmployee(Guid employeeId, string updatedBy, string updatedIp = "")
        {
            if (UserType != UserType.Staff)
                throw new InvalidOperationException("Only staff users can be linked to employees");

            EmployeeId = employeeId;
            SetUpdated(updatedBy, updatedIp);
            AddDomainEvent(new UserLinkedToEmployeeEvent(Id, employeeId));
        }

        // Login management
        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            LoginAttempts = 0;
            LockedUntil = null;

            AddDomainEvent(new UserLoggedInEvent(Id, Username, LastLoginAt.Value));
        }

        public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
        {
            LoginAttempts++;

            if (LoginAttempts >= maxAttempts)
            {
                LockedUntil = DateTime.UtcNow.Add(lockoutDuration);
                AddDomainEvent(new UserAccountLockedEvent(Id, Username, LockedUntil.Value, LoginAttempts));
            }
            else
            {
                AddDomainEvent(new UserLoginFailedEvent(Id, Username, LoginAttempts));
            }
        }

        public bool IsLockedOut()
        {
            return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
        }

        public void UnlockAccount(string updatedBy, string updatedIp = "")
        {
            if (!IsLockedOut()) return;

            LockedUntil = null;
            LoginAttempts = 0;
            SetUpdated(updatedBy, updatedIp);

            AddDomainEvent(new UserAccountUnlockedEvent(Id, Username));
        }

        // ===== REFRESH TOKEN MANAGEMENT (UPDATED WITH SECURITY FEATURES) =====

        /// <summary>
        /// Adds a new refresh token with optional token family for rotation
        /// </summary>
        public RefreshToken AddRefreshToken(
            string token,
            DateTime expiresAt,
            string createdByIp,
            string? tokenFamily = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot create refresh token for inactive user");

            if (IsLockedOut())
                throw new InvalidOperationException("Cannot create refresh token for locked user");

            var refreshToken = RefreshToken.Create(
                userId: Id,
                token: token,
                expiresAt: expiresAt,
                createdByIp: createdByIp,
                tokenFamily: tokenFamily
            );

            _refreshTokens.Add(refreshToken);

            AddDomainEvent(new RefreshTokenCreatedEvent(Id, refreshToken.Id));
            return refreshToken;
        }

        /// <summary>
        /// Revokes a specific refresh token by ID
        /// </summary>
        public void RevokeRefreshToken(Guid tokenId, string revokedByIp, string? reason = null)
        {
            var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId);
            if (token == null)
                throw new InvalidOperationException($"Refresh token {tokenId} not found");

            token.Revoke(revokedByIp, reason);
            AddDomainEvent(new RefreshTokenRevokedEvent(Id, tokenId, reason));
        }

        /// <summary>
        /// Revokes all refresh tokens in a specific family (for security breaches)
        /// </summary>
        public void RevokeTokenFamily(string tokenFamily, string revokedByIp, string reason = "Token family compromised")
        {
            var familyTokens = _refreshTokens
                .Where(rt => rt.BelongsToFamily(tokenFamily) && !rt.IsRevoked)
                .ToList();

            if (!familyTokens.Any())
                return;

            foreach (var token in familyTokens)
            {
                token.Revoke(revokedByIp, reason);
                AddDomainEvent(new RefreshTokenRevokedEvent(Id, token.Id, reason));
            }

            // Raise security event
            AddDomainEvent(new SecurityViolationDetectedEvent(
                Id,
                "TokenFamilyCompromised",
                $"Token family {tokenFamily} compromised. {familyTokens.Count} tokens revoked.",
                revokedByIp));
        }

        /// <summary>
        /// Revokes all active refresh tokens (used for logout all devices)
        /// </summary>
        public void RevokeAllRefreshTokens(string revokedByIp, string reason = "User logged out from all devices")
        {
            var activeTokens = _refreshTokens.Where(rt => rt.IsActive).ToList();

            if (!activeTokens.Any())
                return;

            foreach (var token in activeTokens)
            {
                token.Revoke(revokedByIp, reason);
                AddDomainEvent(new RefreshTokenRevokedEvent(Id, token.Id, reason));
            }
        }

        /// <summary>
        /// Removes expired refresh tokens for cleanup (only removes old expired tokens)
        /// </summary>
        public void RemoveExpiredRefreshTokens()
        {
            // Only remove tokens that expired more than 30 days ago for audit trail
            var expiredTokens = _refreshTokens
                .Where(t => t.IsExpired && t.ExpiryDate.AddDays(30) < DateTime.UtcNow)
                .ToList();

            foreach (var token in expiredTokens)
            {
                _refreshTokens.Remove(token);
            }
        }

        /// <summary>
        /// Gets an active refresh token by token value
        /// </summary>
        public RefreshToken? GetActiveRefreshToken(string token)
        {
            return _refreshTokens.FirstOrDefault(t =>
                t.Token == token &&
                t.IsActive);
        }

        /// <summary>
        /// Gets all active refresh tokens for this user
        /// </summary>
        public IReadOnlyList<RefreshToken> GetActiveRefreshTokens()
        {
            return _refreshTokens
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets refresh token count for monitoring
        /// </summary>
        public int GetActiveTokenCount()
        {
            return _refreshTokens.Count(t => t.IsActive);
        }

        /// <summary>
        /// Checks if user has too many active sessions (security check)
        /// </summary>
        public bool HasTooManySessions(int maxSessions = 5)
        {
            return GetActiveTokenCount() > maxSessions;
        }
    }
}
