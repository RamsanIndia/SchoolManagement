using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus.Events
{
    public class UserLoggedInIntegrationEvent : IEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime LoginTime { get; }

        public UserLoggedInIntegrationEvent(
            Guid userId,
            string username,
            DateTime loginTime,
            DateTime occurredOn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = occurredOn;
            UserId = userId;
            Username = username;
            LoginTime = loginTime;
        }
    }
}
