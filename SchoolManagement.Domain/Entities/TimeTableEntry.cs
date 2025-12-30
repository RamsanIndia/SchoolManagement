using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.ValueObjects;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class TimeTableEntry : BaseEntity
    {
        public Guid SectionId { get; private set; }
        public Guid SubjectId { get; private set; }
        public Guid TeacherId { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public int PeriodNumber { get; private set; }
        public TimePeriod TimePeriod { get; private set; }
        public RoomNumber RoomNumber { get; private set; }

        // Navigation properties
        public virtual Section Section { get; private set; }

        private TimeTableEntry() { }

        public static TimeTableEntry Create(
            Guid sectionId,
            Guid subjectId,
            Guid teacherId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            TimeSpan startTime,
            TimeSpan endTime,
            string roomNumber)
        {
            ValidateTimeTableEntry(dayOfWeek, periodNumber, startTime, endTime);

            var entry = new TimeTableEntry
            {
                Id = Guid.NewGuid(),
                SectionId = sectionId,
                SubjectId = subjectId,
                TeacherId = teacherId,
                DayOfWeek = dayOfWeek,
                PeriodNumber = periodNumber,
                TimePeriod = new TimePeriod(startTime, endTime),
                RoomNumber = RoomNumber.Create(roomNumber),
                CreatedAt = DateTime.UtcNow
            };

            entry.AddDomainEvent(new TimeTableEntryCreatedDomainEvent(
                entry.Id,
                entry.SectionId,
                entry.SubjectId,
                entry.TeacherId,
                entry.DayOfWeek,
                entry.PeriodNumber,
                entry.TimePeriod.StartTime,
                entry.TimePeriod.EndTime,
                entry.RoomNumber.Value
            ));

            return entry;
        }

        public void UpdateSchedule(
            Guid subjectId,
            Guid teacherId,
            TimeSpan startTime,
            TimeSpan endTime,
            string roomNumber)
        {
            ValidateScheduleUpdate(startTime, endTime);

            var oldSubjectId = SubjectId;
            var oldTeacherId = TeacherId;
            var oldTimePeriod = TimePeriod;
            var oldRoomNumber = RoomNumber;

            SubjectId = subjectId;
            TeacherId = teacherId;
            TimePeriod = new TimePeriod(startTime, endTime);
            RoomNumber = RoomNumber.Create(roomNumber);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new TimeTableEntryUpdatedDomainEvent(
                Id,
                SectionId,
                oldSubjectId,
                subjectId,
                oldTeacherId,
                teacherId,
                oldTimePeriod.StartTime,
                oldTimePeriod.EndTime,
                TimePeriod.StartTime,
                TimePeriod.EndTime,
                oldRoomNumber.Value,
                RoomNumber.Value
            ));
        }

        public void ChangeTeacher(Guid newTeacherId)
        {
            if (newTeacherId == Guid.Empty)
                throw new ArgumentException("Teacher ID cannot be empty", nameof(newTeacherId));

            if (TeacherId == newTeacherId)
                return;

            var oldTeacherId = TeacherId;
            TeacherId = newTeacherId;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new TeacherAssignedToTimeTableDomainEvent(
                Id,
                SectionId,
                SubjectId,
                oldTeacherId,
                newTeacherId,
                DayOfWeek,
                PeriodNumber
            ));
        }

        public void Cancel()
        {
            if (IsDeleted)
                throw new InvalidOperationException("TimeTable entry is already cancelled");

            IsDeleted = true;
            //DeletedAt = DateTime.UtcNow;

            AddDomainEvent(new TimeTableEntryCancelledDomainEvent(
                Id,
                SectionId,
                SubjectId,
                TeacherId,
                DayOfWeek,
                PeriodNumber
            ));
        }

        private static void ValidateTimeTableEntry(
            DayOfWeek dayOfWeek,
            int periodNumber,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
                throw new ArgumentException("Invalid day of week", nameof(dayOfWeek));

            if (periodNumber <= 0)
                throw new ArgumentException("Period number must be greater than zero", nameof(periodNumber));

            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time");

            if (endTime - startTime < TimeSpan.FromMinutes(30))
                throw new ArgumentException("Period duration must be at least 30 minutes");
        }

        private static void ValidateScheduleUpdate(TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time");

            if (endTime - startTime < TimeSpan.FromMinutes(30))
                throw new ArgumentException("Period duration must be at least 30 minutes");
        }
    }
}
