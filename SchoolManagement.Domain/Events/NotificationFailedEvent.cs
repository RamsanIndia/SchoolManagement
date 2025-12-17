using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public record NotificationFailedEvent(
        Guid NotificationId,
        NotificationType Channel,
        DateTime FailedAt,
        string ErrorMessage,
        int RetryCount,
        int MaxRetries) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
