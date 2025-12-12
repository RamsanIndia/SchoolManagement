using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class OfflineAttendanceRecord : BaseEntity
    {
        // Properties with private setters for encapsulation
        public Guid DeviceId { get; private set; }
        public string EmployeeCode { get; private set; }
        public string StudentCode { get; private set; }
        public DateTime CheckInTime { get; private set; }
        public bool IsSynced { get; private set; }
        public DateTime? SyncedAt { get; private set; }
        public string SyncError { get; private set; }
        public int RetryCount { get; private set; }
        public bool IsDeleted { get; set; }

        // Navigation property
        public virtual BiometricDevice Device { get; private set; }

        // Private constructor for EF Core
        private OfflineAttendanceRecord() : base() { }

        // Factory method to create new record
        public static OfflineAttendanceRecord Create(
            Guid deviceId,
            string studentCode,
            string employeeCode,
            DateTime checkInTime,
            string createdBy,
            string createdIp)
        {
            var record = new OfflineAttendanceRecord
            {
                DeviceId = deviceId,
                EmployeeCode = employeeCode,
                StudentCode = studentCode,
                CheckInTime = checkInTime,
                IsSynced = false,
                RetryCount = 0
            };

            record.SetCreated(createdBy, createdIp);

            // Raise domain event
            record.AddDomainEvent(new OfflineAttendanceRecordCreatedEvent(
                record.Id,
                deviceId,
                studentCode,
                checkInTime));

            return record;
        }

        // Business logic methods
        public void MarkAsSynced(string user = "SYSTEM")
        {
            if (IsSynced)
                return;

            IsSynced = true;
            SyncedAt = DateTime.UtcNow;
            SyncError = null;
            SetUpdated(UpdatedBy,CreatedIP);

            // Raise domain event
            AddDomainEvent(new OfflineAttendanceRecordSyncedEvent(
                Id,
                DeviceId,
                StudentCode,
                CheckInTime));
        }

        public void MarkSyncFailed(string error, string user = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Sync error message cannot be empty", nameof(error));

            SyncError = error;
            RetryCount++;
            SetUpdated(UpdatedBy, CreatedIP);

            // Raise domain event
            AddDomainEvent(new OfflineAttendanceRecordSyncFailedEvent(
                Id,
                DeviceId,
                StudentCode,
                error,
                RetryCount));
        }

        public void UpdateCheckInTime(DateTime newCheckInTime, string user)
        {
            if (IsSynced)
                throw new InvalidOperationException("Cannot update check-in time for already synced record");

            CheckInTime = newCheckInTime;
            SetUpdated(UpdatedBy, CreatedIP);
        }

        public void ResetSyncStatus(string user = "SYSTEM")
        {
            IsSynced = false;
            SyncedAt = null;
            SyncError = null;
            SetUpdated(UpdatedBy, CreatedIP);
        }

        public bool CanRetrySync(int maxRetries = 3)
        {
            return !IsSynced && RetryCount < maxRetries;
        }

        public void SetSynced(bool value)
        {
            IsSynced = value;
        }

        public void MarkAsDeleted(bool value)
        {
            
            IsDeleted = value;
        }
    }
}
