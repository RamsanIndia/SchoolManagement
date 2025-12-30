using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.AcademicYears.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Handlers.Commands
{
    public class DeactivateAcademicYearHandler : IRequestHandler<DeactivateAcademicYearCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeactivateAcademicYearHandler> _logger;

        public DeactivateAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeactivateAcademicYearHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeactivateAcademicYearCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYear = await _unitOfWork.AcademicYearRepository.GetByIdAsync(request.Id, cancellationToken);

                if (academicYear == null || academicYear.IsDeleted)
                {
                    return Result<bool>.Failure("Academic year not found", "AcademicYearNotFound");
                }

                if (!academicYear.IsActive)
                {
                    return Result<bool>.Failure("Academic year is already inactive", "AlreadyInactive");
                }

                if (academicYear.IsCurrent)
                {
                    return Result<bool>.Failure("Cannot deactivate the current academic year", "CannotDeactivateCurrent");
                }

                academicYear.Deactivate(_currentUserService.Username);
                _unitOfWork.AcademicYearRepository.UpdateAsync(academicYear);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Academic year deactivated: {AcademicYearId} by {User}",
                    academicYear.Id, _currentUserService.Username);

                return Result<bool>.Success(true, "Academic year deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating academic year {Id}", request.Id);
                return Result<bool>.Failure("An error occurred while deactivating the academic year", "DeactivationFailed");
            }
        }
    }
}
