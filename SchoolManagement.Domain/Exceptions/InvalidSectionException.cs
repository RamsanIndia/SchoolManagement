using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class InvalidSectionException : DomainException
    {
        public Guid SectionId { get; }

        public InvalidSectionException(Guid sectionId)
            : base($"Section with ID {sectionId} is invalid or does not exist")
        {
            SectionId = sectionId;
        }

        public InvalidSectionException(string message)
            : base(message)
        {
        }
    }
}
