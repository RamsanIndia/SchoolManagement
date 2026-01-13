// Domain/Events/RefreshTokenRevokedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class RefreshTokenRevokedEvent : IDomainEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid TokenId { get; set; }
        public string Reason { get; set; }
        public Guid TenantId { get; set; }
        public Guid SchoolId { get; set; }
        public DateTime OccurredOn { get; set; }


        // Parameterless constructor for deserialization
        public RefreshTokenRevokedEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        // Main constructor
        public RefreshTokenRevokedEvent(Guid userId, Guid tokenId, string reason, Guid tenantId, Guid schoolId)
            : this()
        {
            UserId = userId;
            TokenId = tokenId;
            Reason = reason;
            TenantId = tenantId;
            SchoolId = schoolId;
        }
    }
}
