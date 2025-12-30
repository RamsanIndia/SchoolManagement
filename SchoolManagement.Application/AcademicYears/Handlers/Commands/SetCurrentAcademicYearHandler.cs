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
    public class SetCurrentAcademicYearHandler : IRequestHandler<SetCurrentAcademicYearCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SetCurrentAcademicYearHandler> _logger;

        public SetCurrentAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<SetCurrentAcademicYearHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(SetCurrentAcademicYearCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYear = await _unitOfWork.AcademicYearRepository.GetByIdAsync(request.Id, cancellationToken);

                if (academicYear == null || academicYear.IsDeleted)
                {
                    return Result<bool>.Failure("Academic year not found", "AcademicYearNotFound");
                }

                // Remove current flag from all other academic years
                var currentYears = await _unitOfWork.AcademicYearRepository.GetCurrentYearsAsync(cancellationToken);

                foreach (var year in currentYears)
                {
                    year.RemoveAsCurrent(_currentUserService.Username);
                    _unitOfWork.AcademicYearRepository.UpdateAsync(year);
                }

                // Set this as current
                academicYear.SetAsCurrent(_currentUserService.Username);
                _unitOfWork.AcademicYearRepository.UpdateAsync(academicYear);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Academic year set as current: {AcademicYearId} by {User}",
                    academicYear.Id, _currentUserService.Username);

                return Result<bool>.Success(true, "Academic year set as current successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current academic year {Id}", request.Id);
                return Result<bool>.Failure("An error occurred while setting current academic year", "SetCurrentFailed");
            }
        }
    }
}
