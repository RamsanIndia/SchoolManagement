// Domain/Events/RefreshTokenCreatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class RefreshTokenCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid TokenId { get; set; }
        public Guid TenantId { get; set; }
        public Guid SchoolId { get; set; }
        public DateTime OccurredOn { get; set; }

        // Parameterless constructor for deserialization
        public RefreshTokenCreatedEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        // Main constructor
        public RefreshTokenCreatedEvent(Guid userId, Guid tokenId, Guid tenantId, Guid schoolId)
            : this()
        {
            UserId = userId;
            TokenId = tokenId;
            TenantId = tenantId;
            SchoolId = schoolId;
        }
    }
}
