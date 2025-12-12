using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus.Events
{
    public class UserAccountLockedIntegrationEvent : IEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime LockedUntil { get; }
        public int FailedAttempts { get; }

        public UserAccountLockedIntegrationEvent(
            Guid userId,
            string username,
            DateTime lockedUntil,
            int failedAttempts,
            DateTime occurredOn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = occurredOn;
            UserId = userId;
            Username = username;
            LockedUntil = lockedUntil;
            FailedAttempts = failedAttempts;
        }
    }
}
