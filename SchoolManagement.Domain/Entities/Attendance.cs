// Domain/Entities/Attendance.cs
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public Guid StudentId { get; private set; }
        public DateTime Date { get; private set; }
        public DateTime? CheckInTime { get; private set; }
        public DateTime? CheckOutTime { get; private set; }
        public AttendanceStatus Status { get; private set; }
        public bool IsFromBiometric { get; private set; }
        public Guid? BiometricDeviceId { get; private set; }
        public string Remarks { get; private set; }

        // Navigation properties
        public virtual Student Student { get; private set; }
        public virtual BiometricDevice BiometricDevice { get; private set; }

        // Private constructor for EF Core
        private Attendance() : base() { }

        // Factory method to create attendance record
        public static Attendance Create(
            Guid studentId,
            DateTime date,
            AttendanceStatus status,
            string createdBy,
            string createdIp,
            DateTime? checkInTime = null,
            bool isFromBiometric = false,
            Guid? biometricDeviceId = null,
            string remarks = null)
        {
            if (studentId == Guid.Empty)
                throw new ArgumentException("Student ID is required", nameof(studentId));

            if (!Enum.IsDefined(typeof(AttendanceStatus), status))
                throw new ArgumentException("Invalid attendance status", nameof(status));

            var attendance = new Attendance
            {
                StudentId = studentId,
                Date = date.Date,
                CheckInTime = checkInTime,
                Status = status,
                IsFromBiometric = isFromBiometric,
                BiometricDeviceId = biometricDeviceId,
                Remarks = remarks
            };

            attendance.SetCreated(createdBy, createdIp);

            // Raise domain event
            attendance.AddDomainEvent(new AttendanceMarkedEvent(
                attendance.Id,
                studentId,
                date.Date,
                AttendanceStatus.Present,
                isFromBiometric,
                checkInTime));

            return attendance;
        }

        // Business logic methods
        public void MarkCheckOut(DateTime checkOutTime, string updatedBy, string updatedIp = "Unknown")
        {
            if (CheckOutTime.HasValue)
                throw new InvalidOperationException("Check-out already recorded");

            if (checkOutTime < CheckInTime)
                throw new InvalidOperationException("Check-out time cannot be before check-in time");

            CheckOutTime = checkOutTime;
            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new AttendanceCheckOutMarkedEvent(Id, StudentId, checkOutTime));
        }

        public void UpdateStatus(AttendanceStatus newStatus, string updatedBy, string updatedIp = "Unknown", string reason = null)
        {
            if (!Enum.IsDefined(typeof(AttendanceStatus), newStatus))
                throw new ArgumentException("Invalid attendance status", nameof(newStatus));

            var oldStatus = Status;
            Status = newStatus;
            Remarks = reason ?? Remarks;

            SetUpdated(updatedBy, updatedIp);

            //AddDomainEvent(new AttendanceStatusUpdatedEvent(Id, StudentId, oldStatus.ToString(), newStatus.ToString(), reason));
        }

        public void AddRemarks(string remarks, string updatedBy, string updatedIp = "Unknown")
        {
            if (string.IsNullOrWhiteSpace(remarks))
                throw new ArgumentException("Remarks cannot be empty", nameof(remarks));

            Remarks = remarks;
            SetUpdated(updatedBy, updatedIp);
        }

        public void UpdateCheckInTime(DateTime checkInTime, string updatedBy, string updatedIp = "Unknown")
        {
            if (CheckOutTime.HasValue && checkInTime > CheckOutTime)
                throw new InvalidOperationException("Check-in time cannot be after check-out time");

            CheckInTime = checkInTime;
            SetUpdated(updatedBy, updatedIp);
        }

        public bool IsPresent() => Status == AttendanceStatus.Present || Status == AttendanceStatus.Late;
        public bool IsAbsent() => Status == AttendanceStatus.Absent;
        public bool IsLate() => Status == AttendanceStatus.Late;
        public bool IsExcused() => Status == AttendanceStatus.Excused;
    }
}
