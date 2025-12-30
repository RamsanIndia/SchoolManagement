using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public sealed class PeriodNumberValidationRule
        : IValidationRule<CheckSlotAvailabilityQuery>
    {
        private const int MaxPeriods = 10;

        public Task ValidateAsync(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PeriodNumber <= 0 || request.PeriodNumber > MaxPeriods)
            {
                throw new InvalidPeriodNumberException(
                    request.PeriodNumber, MaxPeriods);
            }

            return Task.CompletedTask;
        }
    }
    
}
