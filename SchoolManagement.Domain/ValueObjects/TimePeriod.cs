using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class TimePeriod : ValueObject
    {
        private static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan MaximumDuration = TimeSpan.FromHours(3);

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public TimeSpan Duration => EndTime - StartTime;

        private TimePeriod() { }

        public TimePeriod(TimeSpan startTime, TimeSpan endTime)
        {
            ValidateTimePeriod(startTime, endTime);

            StartTime = startTime;
            EndTime = endTime;
        }

        public static TimePeriod Create(TimeSpan startTime, TimeSpan endTime)
        {
            return new TimePeriod(startTime, endTime);
        }

        private static void ValidateTimePeriod(TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromHours(24))
                throw new InvalidTimePeriodException(
                    $"Start time {startTime} must be between 00:00 and 23:59");

            if (endTime < TimeSpan.Zero || endTime > TimeSpan.FromHours(24))
                throw new InvalidTimePeriodException(
                    $"End time {endTime} must be between 00:00 and 24:00");

            if (startTime >= endTime)
                throw new InvalidTimePeriodException(
                    $"Start time {startTime} must be before end time {endTime}");

            var duration = endTime - startTime;

            if (duration < MinimumDuration)
                throw new MinimumPeriodDurationException(duration, MinimumDuration);

            if (duration > MaximumDuration)
                throw new InvalidTimePeriodException(
                    $"Period duration of {duration.TotalMinutes} minutes exceeds maximum allowed {MaximumDuration.TotalMinutes} minutes");
        }

        public bool OverlapsWith(TimePeriod other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return StartTime < other.EndTime && EndTime > other.StartTime;
        }

        public bool Contains(TimeSpan time)
        {
            return time >= StartTime && time < EndTime;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartTime;
            yield return EndTime;
        }

        public override string ToString() => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm} ({Duration.TotalMinutes} mins)";
    }
}
