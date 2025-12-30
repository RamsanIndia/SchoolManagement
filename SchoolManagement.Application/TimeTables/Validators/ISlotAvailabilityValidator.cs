using SchoolManagement.Application.TimeTables.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public interface ISlotAvailabilityValidator
    {
        Task ValidateAsync(
            CheckSlotAvailabilityQuery query,
            CancellationToken cancellationToken);
    }
}
