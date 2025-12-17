using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class GradeSubmittedEvent : DomainEvent
    {
        public GradeSubmittedEvent(
            Guid studentId,
            string subjectId,
            string subjectName,
            decimal marks,
            decimal maxMarks)
        {
            StudentId = studentId;
            SubjectId = subjectId;
            SubjectName = subjectName;
            Marks = marks;
            MaxMarks = maxMarks;
        }

        public Guid StudentId { get; }
        public string SubjectId { get; }
        public string SubjectName { get; }
        public decimal Marks { get; }
        public decimal MaxMarks { get; }
    }
}
