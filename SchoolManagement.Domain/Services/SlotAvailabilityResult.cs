using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services
{
    public sealed class SlotAvailabilityResult
    {
        public bool IsAvailable { get; }
        public IReadOnlyList<ConflictDetail> Conflicts { get; }

        public SlotAvailabilityResult(
            bool isAvailable,
            IReadOnlyList<ConflictDetail> conflicts)
        {
            IsAvailable = isAvailable;
            Conflicts = conflicts ?? Array.Empty<ConflictDetail>();
        }

        public static SlotAvailabilityResult Available()
            => new(true, Array.Empty<ConflictDetail>());

        public static SlotAvailabilityResult Unavailable(IReadOnlyList<ConflictDetail> conflicts)
            => new(false, conflicts);
    }
}
