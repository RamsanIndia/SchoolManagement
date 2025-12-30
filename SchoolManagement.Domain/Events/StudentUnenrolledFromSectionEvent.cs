using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class StudentUnenrolledFromSectionEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public Guid StudentId { get; }
        public int NewStrength { get; }
        public string UnenrolledBy { get; }

        public StudentUnenrolledFromSectionEvent(Guid sectionId, Guid studentId,
            int newStrength, string unenrolledBy)
        {
            SectionId = sectionId;
            StudentId = studentId;
            NewStrength = newStrength;
            UnenrolledBy = unenrolledBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
