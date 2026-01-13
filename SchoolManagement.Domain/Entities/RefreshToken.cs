// Domain/Entities/RefreshToken.cs
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        // Multi-tenant properties
        public Guid TenantId { get; private set; }
        public Guid SchoolId { get; private set; }

        // Token properties
        public string Token { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public bool IsRevoked { get; private set; } = false;
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? RevokedBy { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? ReasonRevoked { get; private set; }
        public Guid UserId { get; private set; }

        // Token family tracking for rotation and security
        public string TokenFamily { get; private set; }

        // Navigation properties
        public virtual User User { get; private set; }
        public virtual Tenant Tenant { get; private set; }
        public virtual School School { get; private set; }

        // Computed properties
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Private constructor for EF Core
        private RefreshToken() : base() { }

        /// <summary>
        /// Creates a new refresh token with multi-tenant support
        /// </summary>
        public static RefreshToken Create(
            Guid userId,
            Guid tenantId,
            Guid schoolId,
            string token,
            DateTime expiresAt,
            string createdByIp,
            string? tokenFamily = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

            if (schoolId == Guid.Empty)
                throw new ArgumentException("School ID cannot be empty", nameof(schoolId));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

            if (string.IsNullOrWhiteSpace(createdByIp))
                throw new ArgumentException("Created by IP cannot be null or empty", nameof(createdByIp));

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                TenantId = tenantId,
                SchoolId = schoolId,
                Token = token,
                ExpiryDate = expiresAt,
                CreatedIP = createdByIp,
                IsRevoked = false,
                // First token in rotation chain uses its own value as family ID
                // Subsequent rotations inherit the family ID
                TokenFamily = tokenFamily ?? token
            };

            // Set audit fields
            refreshToken.SetCreated("SYSTEM", createdByIp);

            // Raise domain event
            refreshToken.AddDomainEvent(new RefreshTokenCreatedEvent(userId, refreshToken.Id, tenantId, schoolId));

            return refreshToken;
        }



        /// <summary>
        /// Revokes the token with reason and IP tracking
        /// </summary>
        public void Revoke(string revokedByIp, string revokedBy, string? reason = null)
        {
            if (IsRevoked)
                throw new InvalidOperationException("Token is already revoked");

            if (string.IsNullOrWhiteSpace(revokedByIp))
                throw new ArgumentException("Revoked by IP cannot be null or empty", nameof(revokedByIp));

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            RevokedBy = revokedBy;
            ReasonRevoked = reason ?? "Revoked by user or system";

            SetUpdated("SYSTEM", revokedByIp);

            // Raise domain event
            AddDomainEvent(new RefreshTokenRevokedEvent(UserId, Id, ReasonRevoked, TenantId, SchoolId));
        }

        /// <summary>
        /// Marks token as replaced by a new token and revokes it
        /// </summary>
        public void ReplaceWith(string newToken, string replacedByIp)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                throw new ArgumentException("New token cannot be null or empty", nameof(newToken));

            if (string.IsNullOrWhiteSpace(replacedByIp))
                throw new ArgumentException("Replaced by IP cannot be null or empty", nameof(replacedByIp));

            if (IsRevoked)
                throw new InvalidOperationException(
                    "Cannot replace an already revoked token. Possible token reuse attack.");

            ReplacedByToken = newToken;
            Revoke(replacedByIp, "SYSTEM", "Replaced by new token during rotation");

            // Raise domain event
            AddDomainEvent(new RefreshTokenReplacedEvent(UserId, Id, newToken, TenantId, SchoolId));
        }

        /// <summary>
        /// Validates token is not expired
        /// </summary>
        public void ValidateNotExpired()
        {
            if (IsExpired)
                throw new InvalidOperationException(
                    $"Refresh token expired at {ExpiryDate:yyyy-MM-dd HH:mm:ss} UTC");
        }

        /// <summary>
        /// Validates token is active (not revoked and not expired)
        /// </summary>
        public void ValidateActive()
        {
            if (!IsActive)
            {
                if (IsRevoked)
                    throw new InvalidOperationException(
                        $"Refresh token was revoked at {RevokedAt:yyyy-MM-dd HH:mm:ss} UTC. " +
                        $"Reason: {ReasonRevoked ?? "Unknown"}");

                if (IsExpired)
                    throw new InvalidOperationException(
                        $"Refresh token expired at {ExpiryDate:yyyy-MM-dd HH:mm:ss} UTC");

                throw new InvalidOperationException("Refresh token is not active");
            }
        }

        /// <summary>
        /// Validates token belongs to the specified tenant and school
        /// </summary>
        public void ValidateTenantAndSchool(Guid tenantId, Guid schoolId)
        {
            if (TenantId != tenantId)
                throw new InvalidOperationException(
                    $"Token belongs to different tenant. Expected: {tenantId}, Actual: {TenantId}");

            if (SchoolId != schoolId)
                throw new InvalidOperationException(
                    $"Token belongs to different school. Expected: {schoolId}, Actual: {SchoolId}");
        }

        /// <summary>
        /// Checks if this token belongs to the specified token family
        /// </summary>
        public bool BelongsToFamily(string familyId)
        {
            return !string.IsNullOrWhiteSpace(TokenFamily) &&
                   TokenFamily.Equals(familyId, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if token was replaced (part of rotation chain)
        /// </summary>
        public bool WasReplaced()
        {
            return !string.IsNullOrWhiteSpace(ReplacedByToken);
        }
    }
}