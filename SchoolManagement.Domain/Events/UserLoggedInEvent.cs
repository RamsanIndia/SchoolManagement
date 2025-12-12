// Domain/Events/UserLoggedInEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserLoggedInEvent : IDomainEvent
    {

        public Guid EventId { get; } = Guid.NewGuid();
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime LoginTime { get; }
        public DateTime OccurredOn { get; }

        public UserLoggedInEvent(Guid userId, string username, DateTime loginTime)
        {
            UserId = userId;
            Username = username;
            LoginTime = loginTime;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
