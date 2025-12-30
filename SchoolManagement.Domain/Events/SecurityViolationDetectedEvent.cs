using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    /// <summary>
    /// Domain event raised when a security violation is detected
    /// </summary>
    public class SecurityViolationDetectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid UserId { get; }
        public string ViolationType { get; }
        public string Description { get; }
        public string? IpAddress { get; }
        public DateTime DetectedAt { get; }

        public SecurityViolationDetectedEvent(
            Guid userId,
            string violationType,
            string description,
            string? ipAddress = null)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            UserId = userId;
            ViolationType = violationType;
            Description = description;
            IpAddress = ipAddress;
            DetectedAt = DateTime.UtcNow;
        }
    }
}
