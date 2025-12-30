using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid ClassId { get; }
        public string PreviousCode { get; }
        public string NewCode { get; }
        public string PreviousName { get; }
        public string NewName { get; }
        public int PreviousGrade { get; }
        public int NewGrade { get; }
        public DateTime UpdatedAt { get; }
        public string UpdatedBy { get; }

        public ClassUpdatedEvent(
            Guid classId,
            string previousCode,
            string newCode,
            string previousName,
            string newName,
            int previousGrade,
            int newGrade,
            DateTime updatedAt,
            string updatedBy)
        {
            ClassId = classId;
            PreviousCode = previousCode;
            NewCode = newCode;
            PreviousName = previousName;
            NewName = newName;
            PreviousGrade = previousGrade;
            NewGrade = newGrade;
            UpdatedAt = updatedAt;
            UpdatedBy = updatedBy;
        }
    }
}
