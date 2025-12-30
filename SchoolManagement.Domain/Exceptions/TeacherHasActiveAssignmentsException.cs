using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TeacherHasActiveAssignmentsException : DomainException
    {
        public TeacherHasActiveAssignmentsException()
            : base("Teacher has active assignments.")
        {
        }

        public TeacherHasActiveAssignmentsException(string message) : base(message)
        {
        }

        public TeacherHasActiveAssignmentsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
