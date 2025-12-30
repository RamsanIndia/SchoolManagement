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
    public sealed class TeacherExistsValidationRule
        : IValidationRule<CheckSlotAvailabilityQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeacherExistsValidationRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task ValidateAsync(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            var teacher = await _unitOfWork.TeachersRepository
                .GetByIdAsync(request.TeacherId, cancellationToken);

            if (teacher == null)
            {
                throw new InvalidTimeTableEntryException(
                    $"Teacher with ID {request.TeacherId} not found");
            }
        }
    }
}
