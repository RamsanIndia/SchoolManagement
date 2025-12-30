using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TeacherNotAssignedException : DomainException
    {
        public Guid TeacherId { get; }

        public TeacherNotAssignedException(Guid teacherId)
            : base($"Teacher with ID {teacherId} is not assigned to any timetable entry")
        {
            TeacherId = teacherId;
        }
    }
}
