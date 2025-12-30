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
    public class ActivateAcademicYearHandler : IRequestHandler<ActivateAcademicYearCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ActivateAcademicYearHandler> _logger;

        public ActivateAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<ActivateAcademicYearHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(ActivateAcademicYearCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYear = await _unitOfWork.AcademicYearRepository.GetByIdAsync(request.Id, cancellationToken);

                if (academicYear == null || academicYear.IsDeleted)
                {
                    return Result<bool>.Failure("Academic year not found", "AcademicYearNotFound");
                }

                if (academicYear.IsActive)
                {
                    return Result<bool>.Failure("Academic year is already active", "AlreadyActive");
                }

                academicYear.Activate(_currentUserService.Username);
                _unitOfWork.AcademicYearRepository.UpdateAsync(academicYear);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Academic year activated: {AcademicYearId} by {User}",
                    academicYear.Id, _currentUserService.Username);

                return Result<bool>.Success(true, "Academic year activated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating academic year {Id}", request.Id);
                return Result<bool>.Failure("An error occurred while activating the academic year", "ActivationFailed");
            }
        }
    }
}
