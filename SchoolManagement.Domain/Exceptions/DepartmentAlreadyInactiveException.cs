using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class DepartmentAlreadyInactiveException : DomainException
    {
        public DepartmentAlreadyInactiveException() : base("Department is already inactive.")
        {
        }

        public DepartmentAlreadyInactiveException(string message) : base(message)
        {
        }

        public DepartmentAlreadyInactiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
