using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class InvalidSectionNameException : SectionException
    {
        public InvalidSectionNameException(string message) : base(message) { }
    }
}
