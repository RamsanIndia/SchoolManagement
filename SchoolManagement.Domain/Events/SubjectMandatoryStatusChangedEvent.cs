using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class SubjectMandatoryStatusChangedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid SectionSubjectId { get; }
        public Guid SectionId { get; }
        public Guid SubjectId { get; }
        public bool IsMandatory { get; }

        public SubjectMandatoryStatusChangedEvent(Guid sectionSubjectId, Guid sectionId,
            Guid subjectId, bool isMandatory)
        {
            SectionSubjectId = sectionSubjectId;
            SectionId = sectionId;
            SubjectId = subjectId;
            IsMandatory = isMandatory;
        }
    }
}
