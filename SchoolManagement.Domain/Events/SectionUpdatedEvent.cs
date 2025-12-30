using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SectionUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid SectionId { get; }
        public string SectionName { get; }
        public int Capacity { get; }
        public string RoomNumber { get; }
        public string UpdatedBy { get; }

        public SectionUpdatedEvent(Guid sectionId, string sectionName, int capacity,
            string roomNumber)
        {
            SectionId = sectionId;
            SectionName = sectionName;
            Capacity = capacity;
            RoomNumber = roomNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
