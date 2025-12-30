using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SectionActivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public string ActivatedBy { get; }

        public SectionActivatedEvent(Guid sectionId, string activatedBy)
        {
            SectionId = sectionId;
            ActivatedBy = activatedBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
