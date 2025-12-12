// Domain/Events/RefreshTokenReplacedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class RefreshTokenReplacedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid OldTokenId { get; }
        public string NewToken { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public RefreshTokenReplacedEvent(Guid userId, Guid oldTokenId, string newToken)
        {
            UserId = userId;
            OldTokenId = oldTokenId;
            NewToken = newToken;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
