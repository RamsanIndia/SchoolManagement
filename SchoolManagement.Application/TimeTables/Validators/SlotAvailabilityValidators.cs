using FluentValidation;
using SchoolManagement.Application.TimeTables.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public sealed class SlotAvailabilityValidator : ISlotAvailabilityValidator
    {
        private readonly IEnumerable<IValidationRule<CheckSlotAvailabilityQuery>> _rules;

        public SlotAvailabilityValidator(
            IEnumerable<IValidationRule<CheckSlotAvailabilityQuery>> rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public async Task ValidateAsync(
            CheckSlotAvailabilityQuery query,
            CancellationToken cancellationToken)
        {
            foreach (var rule in _rules)
            {
                await rule.ValidateAsync(query, cancellationToken);
            }
        }
    }
}
