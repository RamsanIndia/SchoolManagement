using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus.Events
{
    public class UserEmailVerifiedIntegrationEvent : IEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid UserId { get; }
        public string Email { get; }

        public UserEmailVerifiedIntegrationEvent(Guid userId, string email, DateTime occurredOn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = occurredOn;
            UserId = userId;
            Email = email;
        }
    }
}
