using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public sealed class SectionExistsValidationRule
        : IValidationRule<CheckSlotAvailabilityQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SectionExistsValidationRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task ValidateAsync(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            var section = await _unitOfWork.SectionsRepository
                .GetByIdAsync(request.SectionId, cancellationToken);

            if (section == null)
            {
                throw new InvalidSectionException(
                    $"Section with ID {request.SectionId} not found");
            }
        }
    }
}
