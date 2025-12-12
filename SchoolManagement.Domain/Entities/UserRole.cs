// Domain/Entities/UserRole.cs
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class UserRole : BaseEntity
    {
        private Guid id1;
        private Guid id2;
        private DateTime utcNow;
        private bool v;

        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public bool IsActive { get; private set; }
        public string AssignedBy { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedBy { get; private set; }
        public string RevokeReason { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual Role Role { get; private set; }

        // Private constructor for EF Core
        private UserRole() : base() { }

        public UserRole(Guid userId, Guid roleID, DateTime utcNow, bool v, DateTime? expiresAt)
        {
            UserId = userId;
            RoleId = roleID;
            this.utcNow = utcNow;
            this.v = v;
            ExpiresAt = expiresAt;
        }

        // ✅ Factory method for creating new role assignments
        public static UserRole Create(
            Guid userId,
            Guid roleId,
            string assignedBy,
            DateTime? expiresAt = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

            if (string.IsNullOrWhiteSpace(assignedBy))
                throw new ArgumentException("AssignedBy cannot be null or empty", nameof(assignedBy));

            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future", nameof(expiresAt));

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true,
                AssignedBy = assignedBy
            };

            // ✅ Set audit fields
            userRole.SetCreated(assignedBy, "System");

            // ✅ Raise domain event
            userRole.AddDomainEvent(new UserRoleAssignedEvent(
                userId,
                roleId,
                expiresAt));

            return userRole;
        }

        // ✅ Business logic method to deactivate role
        public void DeactivateRole(string revokedBy, string reason = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Role is already deactivated");

            if (string.IsNullOrWhiteSpace(revokedBy))
                throw new ArgumentException("RevokedBy cannot be null or empty", nameof(revokedBy));

            IsActive = false;
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
            RevokeReason = reason;

            // ✅ Set audit fields
            SetUpdated(revokedBy, "System");

            // ✅ Raise domain event
            AddDomainEvent(new UserRoleRevokedEvent(
                UserId,
                RoleId,
                reason));
        }

        // ✅ Business logic method to extend role expiry
        public void ExtendRole(DateTime newExpiryDate, string extendedBy = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot extend an inactive role");

            if (newExpiryDate <= DateTime.UtcNow)
                throw new ArgumentException("New expiry date must be in the future", nameof(newExpiryDate));

            if (ExpiresAt.HasValue && newExpiryDate <= ExpiresAt.Value)
                throw new ArgumentException("New expiry date must be later than current expiry date", nameof(newExpiryDate));

            var oldExpiryDate = ExpiresAt;
            ExpiresAt = newExpiryDate;

            // ✅ Set audit fields
            SetUpdated(extendedBy ?? "System", "System");

            // ✅ Raise domain event
            AddDomainEvent(new UserRoleExtendedEvent(
                UserId,
                RoleId,
                oldExpiryDate,
                newExpiryDate));
        }

        // ✅ Business logic method to reactivate role
        public void ReactivateRole(string reactivatedBy, DateTime? newExpiresAt = null)
        {
            if (IsActive)
                throw new InvalidOperationException("Role is already active");

            if (string.IsNullOrWhiteSpace(reactivatedBy))
                throw new ArgumentException("ReactivatedBy cannot be null or empty", nameof(reactivatedBy));

            IsActive = true;
            RevokedAt = null;
            RevokedBy = null;
            RevokeReason = null;
            ExpiresAt = newExpiresAt;

            // ✅ Set audit fields
            SetUpdated(reactivatedBy, "System");

            // ✅ Raise domain event
            AddDomainEvent(new UserRoleReactivatedEvent(
                UserId,
                RoleId,
                newExpiresAt));
        }

        // ✅ Business logic method to check if expired
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        }

        // ✅ Business logic method to check if valid
        public bool IsValid()
        {
            return IsActive && !IsExpired();
        }
    }
}
