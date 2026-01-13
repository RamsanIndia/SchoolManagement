// Domain/Events/RefreshTokenReplacedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class RefreshTokenReplacedEvent : IDomainEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid OldTokenId { get; set; }
        public string NewToken { get; set; }
        public Guid TenantId { get; set; }
        public Guid SchoolId { get; set; }
        public DateTime OccurredOn { get; set; }

        // Parameterless constructor for deserialization
        public RefreshTokenReplacedEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        // Main constructor
        public RefreshTokenReplacedEvent(Guid userId, Guid oldTokenId, string newToken, Guid tenantId, Guid schoolId)
            : this()
        {
            UserId = userId;
            OldTokenId = oldTokenId;
            NewToken = newToken;
            TenantId = tenantId;
            SchoolId = schoolId;
        }
    }
}
