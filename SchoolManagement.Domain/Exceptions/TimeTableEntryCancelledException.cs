using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TimeTableEntryCancelledException : DomainException
    {
        public Guid TimeTableEntryId { get; }

        public TimeTableEntryCancelledException(Guid timeTableEntryId)
            : base($"TimeTable entry {timeTableEntryId} has already been cancelled")
        {
            TimeTableEntryId = timeTableEntryId;
        }
    }

}
