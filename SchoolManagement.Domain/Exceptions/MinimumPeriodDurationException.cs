using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class MinimumPeriodDurationException : DomainException
    {
        public TimeSpan ProvidedDuration { get; }
        public TimeSpan MinimumDuration { get; }

        public MinimumPeriodDurationException(TimeSpan providedDuration, TimeSpan minimumDuration)
            : base($"Period duration of {providedDuration.TotalMinutes} minutes is less than the minimum required {minimumDuration.TotalMinutes} minutes")
        {
            ProvidedDuration = providedDuration;
            MinimumDuration = minimumDuration;
        }
    }
}
