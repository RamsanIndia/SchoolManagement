using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SectionDeactivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public string DeactivatedBy { get; }

        public SectionDeactivatedEvent(Guid sectionId, string deactivatedBy)
        {
            SectionId = sectionId;
            DeactivatedBy = deactivatedBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
