using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public sealed class DayOfWeekValidationRule
        : IValidationRule<CheckSlotAvailabilityQuery>
    {
        public Task ValidateAsync(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), request.DayOfWeek))
            {
                throw new InvalidDayOfWeekException(
                    $"Invalid day of week: {request.DayOfWeek}");
            }

            if (request.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new InvalidDayOfWeekException(
                    "Cannot schedule classes on Sunday");
            }

            return Task.CompletedTask;
        }
    }
}
