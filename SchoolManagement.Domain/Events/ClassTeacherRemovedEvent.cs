using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassTeacherRemovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public Guid? PreviousTeacherId { get; }
        public string RemovedBy { get; }

        public ClassTeacherRemovedEvent(Guid sectionId, Guid? previousTeacherId, string removedBy)
        {
            SectionId = sectionId;
            PreviousTeacherId = previousTeacherId;
            RemovedBy = removedBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
