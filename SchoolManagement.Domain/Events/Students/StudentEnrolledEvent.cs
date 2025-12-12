using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events.Students
{
    public class StudentEnrolledEvent : DomainEvent
    {
        public StudentEnrolledEvent(
            Guid studentId,
            string studentName,
            string grade,
            string section,
            string guardianEmail)
        {
            StudentId = studentId;
            StudentName = studentName;
            Grade = grade;
            Section = section;
            GuardianEmail = guardianEmail;
        }

        public Guid StudentId { get; }
        public string StudentName { get; }
        public string Grade { get; }
        public string Section { get; }
        public string GuardianEmail { get; }
    }
}
