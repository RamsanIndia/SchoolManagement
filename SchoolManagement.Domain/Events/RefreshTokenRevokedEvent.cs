// Domain/Events/RefreshTokenRevokedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class RefreshTokenRevokedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid TokenId { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public RefreshTokenRevokedEvent(Guid userId, Guid tokenId, string reason)
        {
            UserId = userId;
            TokenId = tokenId;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
