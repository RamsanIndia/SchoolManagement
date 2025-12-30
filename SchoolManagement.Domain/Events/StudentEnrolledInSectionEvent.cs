using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class StudentEnrolledInSectionEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid SectionId { get; }
        public Guid StudentId { get; }
        public int NewStrength { get; }
        public string EnrolledBy { get; }

        public StudentEnrolledInSectionEvent(Guid sectionId, Guid studentId,
            int newStrength, string enrolledBy)
        {
            SectionId = sectionId;
            StudentId = studentId;
            NewStrength = newStrength;
            EnrolledBy = enrolledBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
