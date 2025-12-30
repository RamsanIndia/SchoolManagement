using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when section capacity is exceeded
    /// </summary>
    public class SectionCapacityExceededException : DomainException
    {
        public SectionCapacityExceededException()
            : base("Section has reached maximum capacity.")
        {
        }

        public SectionCapacityExceededException(string message)
            : base(message)
        {
        }

        public SectionCapacityExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
