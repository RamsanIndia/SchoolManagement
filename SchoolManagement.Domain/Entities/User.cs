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

            // ✅ FIXED: Pass 2 arguments
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
        public void UpdatePassword(string newPasswordHash, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);

            AddDomainEvent(new UserPasswordChangedEvent(Id, Username));
        }

        // Profile management
        public void UpdateProfile(FullName fullName, Email email, string updatedBy)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));

            var emailChanged = !Email.Equals(email);
            Email = email ?? throw new ArgumentNullException(nameof(email));

            if (emailChanged)
            {
                EmailVerified = false;
                AddDomainEvent(new UserEmailChangedEvent(Id, Email.Value));
            }

            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
            AddDomainEvent(new UserProfileUpdatedEvent(Id, FullName.FirstName, FullName.LastName));
        }

        public void UpdatePhoneNumber(PhoneNumber phoneNumber, string updatedBy)
        {
            var phoneChanged = PhoneNumber == null || !PhoneNumber.Equals(phoneNumber);
            PhoneNumber = phoneNumber;

            if (phoneChanged)
            {
                PhoneVerified = false;
                AddDomainEvent(new UserPhoneNumberChangedEvent(Id, phoneNumber?.Value));
            }

            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
        }

        // Account status management
        public void Activate(string updatedBy)
        {
            if (IsActive) return;

            IsActive = true;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
            AddDomainEvent(new UserActivatedEvent(Id, Username));
        }

        public void Deactivate(string updatedBy, string reason = null)
        {
            if (!IsActive) return;

            IsActive = false;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
            AddDomainEvent(new UserDeactivatedEvent(Id, Username, reason));
        }

        // Email and phone verification
        public void VerifyEmail(string updatedBy)
        {
            if (EmailVerified) return;

            EmailVerified = true;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
            AddDomainEvent(new UserEmailVerifiedEvent(Id, Email.Value));
        }

        public void VerifyPhone(string updatedBy)
        {
            if (PhoneVerified) return;

            PhoneVerified = true;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
            AddDomainEvent(new UserPhoneVerifiedEvent(Id, PhoneNumber?.Value));
        }

        // User linking
        public void LinkToStudent(Guid studentId, string updatedBy)
        {
            if (UserType != UserType.Student && UserType != UserType.Parent)
                throw new InvalidOperationException("Only student or parent users can be linked to students");

            StudentId = studentId;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
            AddDomainEvent(new UserLinkedToStudentEvent(Id, studentId));
        }

        public void LinkToEmployee(Guid employeeId, string updatedBy)
        {
            if (UserType != UserType.Staff)
                throw new InvalidOperationException("Only staff users can be linked to employees");

            EmployeeId = employeeId;
            // ✅ FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);
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

        public void UnlockAccount(string updatedBy)
        {
            if (!IsLockedOut()) return;

            LockedUntil = null;
            LoginAttempts = 0;
            // FIXED: Pass 2 arguments
            SetUpdated(updatedBy, string.Empty);

            AddDomainEvent(new UserAccountUnlockedEvent(Id, Username));
        }

        // Refresh token management (aggregate manages its child entities)
        public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string createdByIp)
        {
            // FIXED: Pass 3 arguments including createdByIp
            var refreshToken = RefreshToken.Create(Id, token, expiresAt, createdByIp);
            _refreshTokens.Add(refreshToken);

            AddDomainEvent(new RefreshTokenCreatedEvent(Id, refreshToken.Id));
            return refreshToken;
        }

        public void RevokeRefreshToken(Guid tokenId, string revokedByIp, string reason = null)
        {
            var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId);
            if (token == null)
                throw new InvalidOperationException($"Refresh token {tokenId} not found");

            token.Revoke(revokedByIp, reason);
            AddDomainEvent(new RefreshTokenRevokedEvent(Id, tokenId, reason));
        }

        public void RemoveExpiredRefreshTokens()
        {
            var expiredTokens = _refreshTokens.Where(t => t.IsExpired).ToList();
            foreach (var token in expiredTokens)
            {
                _refreshTokens.Remove(token);
            }
        }

        public RefreshToken GetActiveRefreshToken(string token)
        {
            return _refreshTokens.FirstOrDefault(t =>
                t.Token == token &&
                t.IsActive);
        }
    }
}
