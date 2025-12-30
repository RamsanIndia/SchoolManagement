using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to assign a class teacher when one is already assigned
    /// </summary>
    public class ClassTeacherAlreadyAssignedException : DomainException
    {
        public ClassTeacherAlreadyAssignedException()
            : base("A class teacher is already assigned to this section.")
        {
        }

        public ClassTeacherAlreadyAssignedException(string message)
            : base(message)
        {
        }

        public ClassTeacherAlreadyAssignedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
