using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services.Strategies
{
    public interface IConflictDetectionStrategy
    {
        IReadOnlyList<ConflictDetail> DetectConflicts(
            SlotAvailabilityRequest request,
            ConflictingEntries entries);
    }
}
