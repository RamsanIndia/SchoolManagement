using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class DepartmentAlreadyActiveException : DomainException
    {
        public DepartmentAlreadyActiveException() : base("Department is already active.")
        {
        }

        public DepartmentAlreadyActiveException(string message) : base(message)
        {
        }

        public DepartmentAlreadyActiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
