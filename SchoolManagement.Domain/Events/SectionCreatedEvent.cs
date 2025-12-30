using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SectionCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid SectionId { get; }
        public Guid ClassId { get; }
        public string SectionName { get; }
        public int Capacity { get; }
        public string RoomNumber { get; }

        public SectionCreatedEvent(Guid sectionId, Guid classId, string sectionName,
            int capacity, string roomNumber)
        {
            SectionId = sectionId;
            ClassId = classId;
            SectionName = sectionName;
            Capacity = capacity;
            RoomNumber = roomNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
