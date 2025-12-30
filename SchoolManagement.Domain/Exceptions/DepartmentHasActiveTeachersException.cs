using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class DepartmentHasActiveTeachersException : DomainException
    {
        public DepartmentHasActiveTeachersException()
            : base("Department has active teachers.")
        {
        }

        public DepartmentHasActiveTeachersException(string message) : base(message)
        {
        }

        public DepartmentHasActiveTeachersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
