using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassCapacityUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid ClassId { get; }
        public string Code { get; }
        public int PreviousCapacity { get; }
        public int NewCapacity { get; }
        public DateTime UpdatedAt { get; }
        public string UpdatedBy { get; }

        public ClassCapacityUpdatedEvent(
            Guid classId,
            string code,
            int previousCapacity,
            int newCapacity,
            DateTime updatedAt,
            string updatedBy)
        {
            ClassId = classId;
            Code = code;
            PreviousCapacity = previousCapacity;
            NewCapacity = newCapacity;
            UpdatedAt = updatedAt;
            UpdatedBy = updatedBy;
        }
    }
}
