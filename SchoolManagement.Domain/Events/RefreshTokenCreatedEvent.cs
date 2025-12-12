// Domain/Events/RefreshTokenCreatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class RefreshTokenCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid UserId { get; }
        public Guid TokenId { get; }
        public DateTime OccurredOn { get; }

        public RefreshTokenCreatedEvent(Guid userId, Guid tokenId)
        {
            UserId = userId;
            TokenId = tokenId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
