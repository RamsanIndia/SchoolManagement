// Domain/Entities/RefreshToken.cs
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        // Properties
        public string Token { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public bool IsRevoked { get; private set; } = false;
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? ReasonRevoked { get; private set; }
        public Guid UserId { get; private set; }

        // Navigation property
        public virtual User User { get; private set; }

        // Computed properties
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Private constructor for EF Core
        private RefreshToken() : base() { }

        // ✅ FIXED: Factory method with createdByIp parameter
        public static RefreshToken Create(
            Guid userId,
            string token,
            DateTime expiresAt,
            string createdByIp)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

            if (string.IsNullOrWhiteSpace(createdByIp))
                throw new ArgumentException("Created by IP cannot be null or empty", nameof(createdByIp));

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiryDate = expiresAt,
                //CreatedByIp = createdByIp,
                IsRevoked = false
            };

            // Set audit fields
            refreshToken.SetCreated("SYSTEM", createdByIp);

            // Raise domain event
            refreshToken.AddDomainEvent(new RefreshTokenCreatedEvent(userId, refreshToken.Id));

            return refreshToken;
        }

        // Business logic methods
        public void Revoke(string revokedByIp, string reason = null)
        {
            if (IsRevoked)
                throw new InvalidOperationException("Token is already revoked");

            if (string.IsNullOrWhiteSpace(revokedByIp))
                throw new ArgumentException("Revoked by IP cannot be null or empty", nameof(revokedByIp));

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            ReasonRevoked = reason;

            // ✅ FIXED: Set updated audit fields
            SetUpdated("SYSTEM", revokedByIp);

            // Raise domain event
            AddDomainEvent(new RefreshTokenRevokedEvent(UserId, Id, reason));
        }

        public void ReplaceWith(string newToken, string replacedByIp)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                throw new ArgumentException("New token cannot be null or empty", nameof(newToken));

            if (string.IsNullOrWhiteSpace(replacedByIp))
                throw new ArgumentException("Replaced by IP cannot be null or empty", nameof(replacedByIp));

            ReplacedByToken = newToken;
            Revoke(replacedByIp, "Replaced by new token");

            // Raise domain event
            AddDomainEvent(new RefreshTokenReplacedEvent(UserId, Id, newToken));
        }

        public void ValidateNotExpired()
        {
            if (IsExpired)
                throw new InvalidOperationException($"Refresh token expired at {ExpiryDate}");
        }

        public void ValidateActive()
        {
            if (!IsActive)
            {
                if (IsRevoked)
                    throw new InvalidOperationException($"Refresh token was revoked at {RevokedAt}. Reason: {ReasonRevoked}");
                if (IsExpired)
                    throw new InvalidOperationException($"Refresh token expired at {ExpiryDate}");

                throw new InvalidOperationException("Refresh token is not active");
            }
        }
    }
}
