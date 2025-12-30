using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to deactivate an already inactive section
    /// </summary>
    public class SectionAlreadyInactiveException : DomainException
    {
        public SectionAlreadyInactiveException()
            : base("Section is already inactive.")
        {
        }

        public SectionAlreadyInactiveException(string message)
            : base(message)
        {
        }

        public SectionAlreadyInactiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
