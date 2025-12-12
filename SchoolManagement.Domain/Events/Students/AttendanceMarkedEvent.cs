// Domain/Events/AttendanceMarkedEvent.cs
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;

namespace SchoolManagement.Domain.Events
{
    public class AttendanceMarkedEvent : IDomainEvent
    {
        public Guid AttendanceId { get; }
        public Guid StudentId { get; }
        public DateTime Date { get; }
        public AttendanceStatus Status { get; }
        public bool IsFromBiometric { get; }
        public DateTime? CheckInTime { get; }
        public DateTime OccurredOn { get; }

        public Guid EventId => throw new NotImplementedException();

        public AttendanceMarkedEvent(
            Guid attendanceId,
            Guid studentId,
            DateTime date,
            AttendanceStatus status,
            bool isFromBiometric,
            DateTime? checkInTime)
        {
            AttendanceId = attendanceId;
            StudentId = studentId;
            Date = date;
            Status = status;
            IsFromBiometric = isFromBiometric;
            CheckInTime = checkInTime;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
