using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TeacherAlreadyInactiveException : DomainException
    {
        public TeacherAlreadyInactiveException() : base("Teacher is already inactive.")
        {
        }

        public TeacherAlreadyInactiveException(string message) : base(message)
        {
        }

        public TeacherAlreadyInactiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
