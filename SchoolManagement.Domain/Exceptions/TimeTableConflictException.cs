using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TimeTableConflictException : DomainException
    {
        public Guid ConflictingEntryId { get; }
        public DayOfWeek DayOfWeek { get; }
        public int PeriodNumber { get; }

        public TimeTableConflictException(
            Guid conflictingEntryId,
            DayOfWeek dayOfWeek,
            int periodNumber)
            : base($"TimeTable conflict detected for {dayOfWeek}, Period {periodNumber}. Conflicts with entry {conflictingEntryId}")
        {
            ConflictingEntryId = conflictingEntryId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
        }

        public TimeTableConflictException(
            string message,
            Guid conflictingEntryId,
            DayOfWeek dayOfWeek,
            int periodNumber)
            : base(message)
        {
            ConflictingEntryId = conflictingEntryId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
        }
    }
}
