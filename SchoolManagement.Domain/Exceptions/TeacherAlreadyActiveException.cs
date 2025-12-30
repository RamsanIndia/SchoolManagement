using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TeacherAlreadyActiveException : DomainException
    {
        public TeacherAlreadyActiveException() : base("Teacher is already active.")
        {
        }

        public TeacherAlreadyActiveException(string message) : base(message)
        {
        }

        public TeacherAlreadyActiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
