using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to remove or change a non-existent class teacher
    /// </summary>
    public class NoClassTeacherAssignedException : DomainException
    {
        public NoClassTeacherAssignedException()
            : base("No class teacher is assigned to this section.")
        {
        }

        public NoClassTeacherAssignedException(string message)
            : base(message)
        {
        }

        public NoClassTeacherAssignedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
