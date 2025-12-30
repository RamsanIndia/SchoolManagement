using SchoolManagement.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ISlotAvailabilityApplicationService
    {
        Task<SlotAvailabilityResult> CheckAvailabilityAsync(
            SlotAvailabilityRequest request,
            CancellationToken cancellationToken = default);
    }
}
