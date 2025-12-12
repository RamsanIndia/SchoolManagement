// Domain/Events/OfflineAttendanceRecordSyncFailedEvent.cs
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Domain.Events
{
    public class OfflineAttendanceRecordSyncFailedEvent : IDomainEvent
    {
        public Guid RecordId { get; }
        public Guid DeviceId { get; }
        public string StudentCode { get; }
        public string Error { get; }
        public int RetryCount { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public OfflineAttendanceRecordSyncFailedEvent(
            Guid recordId,
            Guid deviceId,
            string studentCode,
            string error,
            int retryCount)
        {
            RecordId = recordId;
            DeviceId = deviceId;
            StudentCode = studentCode;
            Error = error;
            RetryCount = retryCount;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
