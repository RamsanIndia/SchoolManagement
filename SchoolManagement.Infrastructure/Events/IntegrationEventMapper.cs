using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Infrastructure.EventBus;
using SchoolManagement.Infrastructure.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Events
{
    public class IntegrationEventMapper : IIntegrationEventMapper
    {
        private readonly ILogger<IntegrationEventMapper> _logger;

        public IntegrationEventMapper(ILogger<IntegrationEventMapper> logger)
        {
            _logger = logger;
        }

        public IEvent MapToIntegrationEvent(IDomainEvent domainEvent)
        {
            return domainEvent switch
            {
                UserCreatedEvent e => new UserCreatedIntegrationEvent(
                    e.UserId,
                    e.Username,
                    e.Email,
                    e.UserType.ToString(),
                    e.OccurredOn),

                UserLoggedInEvent e => new UserLoggedInIntegrationEvent(
                    e.UserId,
                    e.Username,
                    e.LoginTime,
                    e.OccurredOn),

                UserAccountLockedEvent e => new UserAccountLockedIntegrationEvent(
                    e.UserId,
                    e.Username,
                    e.LockedUntil,
                    e.FailedAttempts,
                    e.OccurredOn),

                OfflineAttendanceRecordSyncedEvent e => new AttendanceRecordSyncedIntegrationEvent(
                    e.RecordId,
                    e.DeviceId,
                    e.StudentCode,
                    e.CheckInTime,
                    e.OccurredOn),

                UserEmailVerifiedEvent e => new UserEmailVerifiedIntegrationEvent(
                    e.UserId,
                    e.Email,
                    e.OccurredOn),

                // Add more mappings as needed

                _ => null // Domain events without integration event mapping
            };
        }
    }
}
