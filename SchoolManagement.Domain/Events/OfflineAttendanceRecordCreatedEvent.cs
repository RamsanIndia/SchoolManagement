// Domain/Events/OfflineAttendanceRecordCreatedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class OfflineAttendanceRecordCreatedEvent : IDomainEvent
    {
        public Guid RecordId { get; }
        public Guid DeviceId { get; }
        public string StudentCode { get; }
        public DateTime CheckInTime { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public OfflineAttendanceRecordCreatedEvent(
            Guid recordId,
            Guid deviceId,
            string studentCode,
            DateTime checkInTime)
        {
            RecordId = recordId;
            DeviceId = deviceId;
            StudentCode = studentCode;
            CheckInTime = checkInTime;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
