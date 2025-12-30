using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class TeacherInactiveException : DomainException
    {
        public TeacherInactiveException() : base("Teacher is inactive.")
        {
        }

        public TeacherInactiveException(string message) : base(message)
        {
        }

        public TeacherInactiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
