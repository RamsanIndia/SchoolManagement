using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services.Strategies
{
    public sealed class CompositeConflictDetectionStrategy : IConflictDetectionStrategy
    {
        private readonly IEnumerable<IConflictDetector> _detectors;

        public CompositeConflictDetectionStrategy(
            IEnumerable<IConflictDetector> detectors)
        {
            _detectors = detectors ?? throw new ArgumentNullException(nameof(detectors));
        }

        public IReadOnlyList<ConflictDetail> DetectConflicts(
            SlotAvailabilityRequest request,
            ConflictingEntries entries)
        {
            var conflicts = new List<ConflictDetail>();

            foreach (var detector in _detectors)
            {
                var conflict = detector.Detect(request, entries);
                if (conflict != null)
                {
                    conflicts.Add(conflict);
                }
            }

            return conflicts;
        }
    }
}
