using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to activate an already active section
    /// </summary>
    public class SectionAlreadyActiveException : DomainException
    {
        public SectionAlreadyActiveException()
            : base("Section is already active.")
        {
        }

        public SectionAlreadyActiveException(string message)
            : base(message)
        {
        }

        public SectionAlreadyActiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
