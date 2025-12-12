using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus.Events
{
    public class AttendanceRecordSyncedIntegrationEvent : IEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid RecordId { get; }
        public Guid DeviceId { get; }
        public string StudentCode { get; }
        public DateTime CheckInTime { get; }

        public AttendanceRecordSyncedIntegrationEvent(
            Guid recordId,
            Guid deviceId,
            string studentCode,
            DateTime checkInTime,
            DateTime occurredOn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = occurredOn;
            RecordId = recordId;
            DeviceId = deviceId;
            StudentCode = studentCode;
            CheckInTime = checkInTime;
        }
    }
}
