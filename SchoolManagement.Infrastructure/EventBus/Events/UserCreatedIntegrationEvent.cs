using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus.Events
{
    public class UserCreatedIntegrationEvent : IEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid UserId { get; }
        public string Username { get; }
        public string Email { get; }
        public string UserType { get; }

        public UserCreatedIntegrationEvent(
            Guid userId,
            string username,
            string email,
            string userType,
            DateTime occurredOn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = occurredOn;
            UserId = userId;
            Username = username;
            Email = email;
            UserType = userType;
        }
    }
}
