using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class InvalidDayOfWeekException : DomainException
    {
        public string ProvidedDay { get; }

        public InvalidDayOfWeekException(string message)
            : base(message)
        {
        }

        public InvalidDayOfWeekException(string message, string providedDay)
            : base(message)
        {
            ProvidedDay = providedDay;
        }
    }
}
