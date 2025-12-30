using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services.Strategies
{
    public interface IConflictDetector
    {
        ConflictDetail Detect(
            SlotAvailabilityRequest request,
            ConflictingEntries entries);
    }
}
