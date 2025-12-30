using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class SectionException : DomainException
    {
        public SectionException(string message) : base(message) { }
        public SectionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
