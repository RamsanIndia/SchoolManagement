// Domain/Entities/User.cs - COMPLETE REFACTORED MULTI-TENANT VERSION
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        // ===== IDENTITY =====
        public string Username { get; private set; } = string.Empty;
        public Email Email { get; private set; } = null!;
        public FullName FullName { get; private set; } = null!;
        public string PasswordHash { get; private set; } = string.Empty;
        public PhoneNumber? PhoneNumber { get; private set; }

        // ===== SECURITY =====
        public bool EmailVerified { get; private set; }
        public bool PhoneVerified { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public int LoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }

        // ===== DOMAIN =====
        public UserType UserType { get; private set; }
        public Guid? StudentId { get; private set; }
        public Guid? EmployeeId { get; private set; }

        // ===== MULTI-TENANCY (INHERITED FROM BASEENTITY) =====
        // TenantId, SchoolId inherited from BaseEntity
        // IsActive inherited from BaseEntity
        public virtual School? School { get; private set; }

        // ===== TOKENS =====
        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        // ===== NAVIGATION =====
        public virtual Student? Student { get; private set; }
        public virtual Employee? Employee { get; private set; }
        public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        // ===== COMPUTED PROPERTIES =====
        public bool IsLockedOut => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
        public bool CanLogin => IsActive && !IsDeleted && !IsLockedOut;
        public bool HasVerifiedEmail => EmailVerified;
        public bool HasVerifiedPhone => PhoneVerified;
        public bool IsFullyVerified => EmailVerified && (PhoneNumber == null || PhoneVerified);

        // ✅ EF Constructor
        private User() : base() { }

        /// <summary>
        /// Factory method to create a new user with full tenant and school context
        /// </summary>
        public static User Create(
            Guid tenantId,
            Guid schoolId,
            string username,
            Email email,
            FullName fullName,
            string passwordHash,
            UserType userType,
            string createdBy,
            string createdIp)
        {
            // ✅ Validation
            ValidateCreateParams(tenantId, schoolId, username, passwordHash);

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,        // ✅ From BaseEntity
                SchoolId = schoolId,        // ✅ From BaseEntity
                Username = username.Trim(),
                Email = email,
                FullName = fullName,
                PasswordHash = passwordHash,
                UserType = userType,
                EmailVerified = false,
                PhoneVerified = false,
                LoginAttempts = 0,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Set audit info (from BaseEntity)
            user.SetCreated(createdBy, createdIp);
            user.Activate(createdBy); // ✅ Use BaseEntity.Activate()

            // ✅ Domain event WITH full tenant context
            //user.AddDomainEvent(new UserCreatedEvent(
            //    user.Id,
            //    user.TenantId,
            //    user.SchoolId.Value,
            //    user.Username,
            //    user.Email.Value,
            //    userType));

            return user;
        }

        /// <summary>
        /// Factory method for student users
        /// </summary>
        public static User CreateStudentUser(
            Guid tenantId,
            Guid schoolId,
            string username,
            Email email,
            FullName fullName,
            string passwordHash,
            string createdBy,
            string createdIp)
        {
            return Create(tenantId, schoolId, username, email, fullName, passwordHash,
                         UserType.Student, createdBy, createdIp);
        }

        /// <summary>
        /// Factory method for staff users
        /// </summary>
        public static User CreateStaffUser(
            Guid tenantId,
            Guid schoolId,
            string username,
            Email email,
            FullName fullName,
            string passwordHash,
            string createdBy,
            string createdIp)
        {
            return Create(tenantId, schoolId, username, email, fullName, passwordHash,
                         UserType.Staff, createdBy, createdIp);
        }

        /// <summary>
        /// Factory method for parent users
        /// </summary>
        public static User CreateParentUser(
            Guid tenantId,
            Guid schoolId,
            string username,
            Email email,
            FullName fullName,
            string passwordHash,
            string createdBy,
            string createdIp)
        {
            return Create(tenantId, schoolId, username, email, fullName, passwordHash,
                         UserType.Parent, createdBy, createdIp);
        }

        /// <summary>
        /// Factory method for admin users
        /// </summary>
        public static User CreateAdminUser(
            Guid tenantId,
            Guid schoolId,
            string username,
            Email email,
            FullName fullName,
            string passwordHash,
            string createdBy,
            string createdIp)
        {
            return Create(tenantId, schoolId, username, email, fullName, passwordHash,
                         UserType.Admin, createdBy, createdIp);
        }

        // ===== SCHOOL MANAGEMENT =====

        /// <summary>
        /// Assign user to a school (validates tenant hierarchy)
        /// </summary>
        public void AssignSchool(Guid tenantId, Guid schoolId, string updatedBy, string updatedIp = "")
        {
            if (this.TenantId != tenantId)
                throw new DomainException("Cannot assign school from different tenant");

            if (schoolId == Guid.Empty)
                throw new ArgumentException("Invalid school ID", nameof(schoolId));

            SchoolId = schoolId;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserSchoolAssignedEvent(Id, TenantId, schoolId));
        }

        /// <summary>
        /// Transfer user to another school within the same tenant
        /// </summary>
        public void TransferToSchool(Guid newSchoolId, string reason, string updatedBy, string updatedIp = "")
        {
            if (newSchoolId == Guid.Empty)
                throw new ArgumentException("Invalid school ID", nameof(newSchoolId));

            if (SchoolId == newSchoolId)
                throw new InvalidOperationException("User is already assigned to this school");

            var oldSchoolId = SchoolId;
            SchoolId = newSchoolId;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserSchoolTransferredEvent(Id, TenantId, oldSchoolId.Value, newSchoolId, reason));
        }

        /// <summary>
        /// Check if user belongs to a specific school
        /// </summary>
        public bool BelongsToSchool(Guid schoolId) => SchoolId.HasValue && SchoolId == schoolId;

        /// <summary>
        /// Check if user belongs to the same school as another user
        /// </summary>
        public bool IsInSameSchoolAs(User otherUser) =>
            SchoolId.HasValue && otherUser.SchoolId.HasValue && SchoolId == otherUser.SchoolId;

        // ===== SECURITY GUARDS =====

        /// <summary>
        /// Ensure user has valid school context before mutations
        /// </summary>
        private void EnsureSchoolContext(Guid? expectedSchoolId = null)
        {
            if (!SchoolId.HasValue || SchoolId == Guid.Empty)
                throw new InvalidOperationException("User is not assigned to any school");

            if (expectedSchoolId.HasValue && SchoolId != expectedSchoolId.Value)
                throw new UnauthorizedAccessException(
                    $"User belongs to school {SchoolId} but operation requires school {expectedSchoolId}");
        }

        /// <summary>
        /// Ensure user can perform actions
        /// </summary>
        private void EnsureCanPerformAction()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot perform action on deleted user");

            if (!IsActive)
                throw new InvalidOperationException("Cannot perform action on inactive user");

            if (IsLockedOut)
                throw new InvalidOperationException("Cannot perform action on locked user");
        }

        // ===== PASSWORD MANAGEMENT =====

        /// <summary>
        /// Update user password
        /// </summary>
        public void UpdatePassword(string newPasswordHash, string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();
            EnsureCanPerformAction();
            ValidatePasswordHash(newPasswordHash);

            PasswordHash = newPasswordHash;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserPasswordChangedEvent(Id, TenantId, SchoolId.Value, Username));
        }

        // ===== PROFILE MANAGEMENT =====

        /// <summary>
        /// Update user profile
        /// </summary>
        public void UpdateProfile(FullName fullName, Email email, string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();
            EnsureCanPerformAction();

            if (fullName == null)
                throw new ArgumentNullException(nameof(fullName));
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            var emailChanged = !Email.Equals(email);

            FullName = fullName;
            Email = email;

            if (emailChanged)
            {
                EmailVerified = false;
                //AddDomainEvent(new UserEmailChangedEvent(Id, TenantId, SchoolId.Value, Email.Value));
            }

            SetUpdated(updatedBy, updatedIp);
            //AddDomainEvent(new UserProfileUpdatedEvent(Id, TenantId, SchoolId.Value, FullName.FirstName, FullName.LastName));
        }

        /// <summary>
        /// Update phone number
        /// </summary>
        public void UpdatePhoneNumber(PhoneNumber phoneNumber, string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();
            EnsureCanPerformAction();

            var phoneChanged = PhoneNumber == null || !PhoneNumber.Equals(phoneNumber);
            PhoneNumber = phoneNumber;

            if (phoneChanged)
            {
                PhoneVerified = false;
                //AddDomainEvent(new UserPhoneNumberChangedEvent(Id, TenantId, SchoolId.Value, phoneNumber?.Value));
            }

            SetUpdated(updatedBy, updatedIp);
        }

        /// <summary>
        /// Update username
        /// </summary>
        public void UpdateUsername(string newUsername, string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();
            EnsureCanPerformAction();

            if (string.IsNullOrWhiteSpace(newUsername))
                throw new ArgumentException("Username cannot be empty", nameof(newUsername));

            var oldUsername = Username;
            Username = newUsername.Trim();
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserUsernameChangedEvent(Id, TenantId, SchoolId.Value, oldUsername, newUsername));
        }

        // ===== ACCOUNT STATUS =====
        // Note: Activate() and Deactivate() are inherited from BaseEntity, but we can override them

        /// <summary>
        /// Activate user account (overrides BaseEntity method)
        /// </summary>
        public new void Activate(string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();

            if (IsActive && !IsDeleted)
                return;

            base.Activate(updatedBy); // ✅ Call BaseEntity.Activate()
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserActivatedEvent(Id, TenantId, SchoolId.Value, Username));
        }

        /// <summary>
        /// Deactivate user account (overrides BaseEntity method)
        /// </summary>
        public new void Deactivate(string updatedBy, string updatedIp = "", string reason = "")
        {
            EnsureSchoolContext();

            if (!IsActive)
                return;

            base.Deactivate(updatedBy); // ✅ Call BaseEntity.Deactivate()
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserDeactivatedEvent(Id, TenantId, SchoolId.Value, Username, reason));
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        public void VerifyEmail(string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();

            if (EmailVerified)
                return;

            EmailVerified = true;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserEmailVerifiedEvent(Id, TenantId, SchoolId.Value, Email.Value));
        }

        /// <summary>
        /// Verify phone number
        /// </summary>
        public void VerifyPhone(string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();

            if (PhoneVerified)
                return;

            PhoneVerified = true;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserPhoneVerifiedEvent(Id, TenantId, SchoolId.Value, PhoneNumber?.Value));
        }

        // ===== ENTITY LINKING =====

        /// <summary>
        /// Link user to a student entity
        /// </summary>
        public void LinkToStudent(Guid studentId, string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();
            EnsureCanPerformAction();

            if (UserType is not (UserType.Student or UserType.Parent))
                throw new InvalidOperationException("Only students and parents can be linked to student entities");

            if (studentId == Guid.Empty)
                throw new ArgumentException("Invalid student ID", nameof(studentId));

            StudentId = studentId;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserLinkedToStudentEvent(Id, TenantId, SchoolId.Value, studentId));
        }

        /// <summary>
        /// Unlink user from student entity
        /// </summary>
        public void UnlinkFromStudent(string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();

            if (!StudentId.HasValue)
                return;

            var studentId = StudentId.Value;
            StudentId = null;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserUnlinkedFromStudentEvent(Id, TenantId, SchoolId.Value, studentId));
        }

        /// <summary>
        /// Link user to an employee entity
        /// </summary>
        public void LinkToEmployee(Guid employeeId, string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();
            EnsureCanPerformAction();

            if (UserType is not (UserType.Staff or UserType.Admin))
                throw new InvalidOperationException("Only staff and admins can be linked to employee entities");

            if (employeeId == Guid.Empty)
                throw new ArgumentException("Invalid employee ID", nameof(employeeId));

            EmployeeId = employeeId;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserLinkedToEmployeeEvent(Id, TenantId, SchoolId.Value, employeeId));
        }

        /// <summary>
        /// Unlink user from employee entity
        /// </summary>
        public void UnlinkFromEmployee(string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();

            if (!EmployeeId.HasValue)
                return;

            var employeeId = EmployeeId.Value;
            EmployeeId = null;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserUnlinkedFromEmployeeEvent(Id, TenantId, SchoolId.Value, employeeId));
        }

        /// <summary>
        /// Record successful login
        /// </summary>
        public void RecordSuccessfulLogin(string ipAddress)
        {
            LastLoginAt = DateTime.UtcNow;
            LoginAttempts = 0;
            LockedUntil = null;

            //AddDomainEvent(new UserLoggedInEvent(Id, TenantId, SchoolId.Value, Username, LastLoginAt.Value, ipAddress));
        }

        /// <summary>
        /// Record failed login attempt
        /// </summary>
        public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration, string ipAddress)
        {
            LoginAttempts++;

            if (LoginAttempts >= maxAttempts)
            {
                LockedUntil = DateTime.UtcNow.Add(lockoutDuration);
                //AddDomainEvent(new UserAccountLockedEvent(
                //    Id, TenantId, SchoolId.Value, Username, LockedUntil.Value, LoginAttempts, ipAddress));
            }
            else
            {
                //AddDomainEvent(new UserLoginFailedEvent(
                //    Id, TenantId, SchoolId.Value, Username, LoginAttempts, ipAddress));
            }
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        public void UnlockAccount(string updatedBy, string updatedIp = "")
        {
            EnsureSchoolContext();

            if (!IsLockedOut)
                return;

            LockedUntil = null;
            LoginAttempts = 0;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new UserAccountUnlockedEvent(Id, TenantId, SchoolId.Value, Username));
        }

        /// <summary>
        /// Reset failed login attempts
        /// </summary>
        public void ResetLoginAttempts(string updatedBy, string updatedIp = "")
        {
            if (LoginAttempts == 0)
                return;

            LoginAttempts = 0;
            SetUpdated(updatedBy, updatedIp);
        }

        // ===== REFRESH TOKEN MANAGEMENT =====

        /// <summary>
        /// Add a new refresh token
        /// </summary>
        public RefreshToken AddRefreshToken(Guid tenantId,Guid schoolId, string token, DateTime expiresAt, string createdByIp, string? tokenFamily = null)
        {
            if (!CanLogin)
                throw new InvalidOperationException("Cannot create token for inactive, locked, or deleted user");

            // Validate tenant and school
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

            if (schoolId == Guid.Empty)
                throw new ArgumentException("School ID cannot be empty", nameof(schoolId));

            // Create refresh token with tenant and school context
            var refreshToken = RefreshToken.Create(
                userId: Id,
                tenantId: TenantId.Value,
                schoolId: SchoolId.Value,
                token: token,
                expiresAt: expiresAt,
                createdByIp: createdByIp,
                tokenFamily: tokenFamily
            );

            _refreshTokens.Add(refreshToken);

            // Domain event is already raised inside RefreshToken.Create()
            // No need to add it again here to avoid duplicate events

            return refreshToken;
        }

        /// <summary>
        /// Revoke a specific refresh token
        /// </summary>
        public void RevokeRefreshToken(Guid tokenId, string revokedByIp, string? reason = null)
        {
            var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId)
                ?? throw new InvalidOperationException($"Token {tokenId} not found");

            token.Revoke(revokedByIp, reason);

            //AddDomainEvent(new RefreshTokenRevokedEvent(Id, TenantId, SchoolId.Value, tokenId, reason));
        }

        /// <summary>
        /// Revoke entire token family (security measure)
        /// </summary>
        public void RevokeTokenFamily(string tokenFamily, string revokedByIp, string reason = "Compromised")
        {
            var familyTokens = _refreshTokens
                .Where(rt => rt.BelongsToFamily(tokenFamily) && !rt.IsRevoked)
                .ToList();

            foreach (var token in familyTokens)
            {
                token.Revoke(revokedByIp, reason);
                //AddDomainEvent(new RefreshTokenRevokedEvent(Id, TenantId, SchoolId.Value, token.Id, reason));
            }

            if (familyTokens.Any())
            {
                //AddDomainEvent(new SecurityViolationDetectedEvent(
                //    Id, TenantId, SchoolId.Value, "TokenFamilyCompromised",
                //    $"Revoked {familyTokens.Count} tokens from family {tokenFamily}", revokedByIp));
            }
        }

        /// <summary>
        /// Revoke all refresh tokens (logout all devices)
        /// </summary>
        public void RevokeAllRefreshTokens(string revokedByIp, string reason = "Logout all devices")
        {
            var activeTokens = _refreshTokens.Where(rt => rt.IsActive).ToList();

            foreach (var token in activeTokens)
            {
                token.Revoke(revokedByIp, reason);
                //AddDomainEvent(new RefreshTokenRevokedEvent(Id, TenantId, SchoolId.Value, token.Id, reason));
            }
        }

        /// <summary>
        /// Remove expired tokens (cleanup)
        /// </summary>
        public void RemoveExpiredRefreshTokens()
        {
            var expired = _refreshTokens
                .Where(t => t.IsExpired && t.ExpiryDate.AddDays(30) < DateTime.UtcNow)
                .ToList();

            foreach (var token in expired)
            {
                _refreshTokens.Remove(token);
            }
        }

        /// <summary>
        /// Get active refresh token by token string
        /// </summary>
        public RefreshToken? GetActiveRefreshToken(string token) =>
            _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);

        /// <summary>
        /// Get all active refresh tokens
        /// </summary>
        public IReadOnlyList<RefreshToken> GetActiveRefreshTokens() =>
            _refreshTokens
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .AsReadOnly();

        /// <summary>
        /// Get count of active sessions
        /// </summary>
        public int GetActiveTokenCount() => _refreshTokens.Count(t => t.IsActive);

        /// <summary>
        /// Check if user has too many active sessions
        /// </summary>
        public bool HasTooManySessions(int maxSessions = 5) => GetActiveTokenCount() > maxSessions;

        // ===== PRIVATE VALIDATORS =====

        private static void ValidateCreateParams(Guid tenantId, Guid schoolId, string username, string passwordHash)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId is required", nameof(tenantId));
            if (schoolId == Guid.Empty)
                throw new ArgumentException("SchoolId is required", nameof(schoolId));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required", nameof(username));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash is required", nameof(passwordHash));
        }

        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));
        }

        // ===== EQUALITY =====
        public override bool Equals(object obj) => obj is User other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}