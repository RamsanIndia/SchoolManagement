using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class InvalidPeriodNumberException : DomainException
    {
        public int ProvidedPeriodNumber { get; }
        public int MaxAllowedPeriods { get; }

        public InvalidPeriodNumberException(int providedPeriodNumber, int maxAllowedPeriods)
            : base($"Period number {providedPeriodNumber} is invalid. Must be between 1 and {maxAllowedPeriods}")
        {
            ProvidedPeriodNumber = providedPeriodNumber;
            MaxAllowedPeriods = maxAllowedPeriods;
        }
    }

}
