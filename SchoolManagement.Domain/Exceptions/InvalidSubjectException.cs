using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class InvalidSubjectException : DomainException
    {
        public Guid SubjectId { get; }

        public InvalidSubjectException(Guid subjectId)
            : base($"Subject with ID {subjectId} is invalid or does not exist")
        {
            SubjectId = subjectId;
        }

        public InvalidSubjectException(string message)
            : base(message)
        {
        }
    }
}
