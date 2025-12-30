using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class DepartmentException : DomainException
    {
        public DepartmentException() : base("Department operation failed.")
        {
        }

        public DepartmentException(string message) : base(message)
        {
        }

        public DepartmentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
