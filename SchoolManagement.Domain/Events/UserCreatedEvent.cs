using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;

namespace SchoolManagement.Domain.Events
{
    public class UserCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();  // Implement this
        public DateTime OccurredOn { get; } = DateTime.UtcNow;  // Implement this if needed

        public Guid UserId { get; }
        public string Username { get; }
        public string Email { get; }
        public UserType UserType { get; }
        public Guid SchoolId { get; set; }
        public Guid TenantId { get; set; }

        public UserCreatedEvent(Guid userId, string username, string email, UserType userType,Guid tenantId, Guid schoolId)
        {
            UserId = userId;
            Username = username;
            Email = email;
            UserType = userType;
            SchoolId = schoolId;
            TenantId = tenantId;
        }
    }
}
