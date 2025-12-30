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
    public class UpdateAcademicYearHandler : IRequestHandler<UpdateAcademicYearCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateAcademicYearHandler> _logger;

        public UpdateAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<UpdateAcademicYearHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateAcademicYearCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYear = await _unitOfWork.AcademicYearRepository.GetByIdAsync(request.Id, cancellationToken);

                if (academicYear == null || academicYear.IsDeleted)
                {
                    return Result<bool>.Failure("Academic year not found", "AcademicYearNotFound");
                }

                // Check for duplicate name
                var duplicateExists = await _unitOfWork.AcademicYearRepository.ExistsAsync(request.Name, request.Id, cancellationToken);

                if (duplicateExists)
                {
                    return Result<bool>.Failure("Academic year with this name already exists", "AcademicYearExists");
                }

                // Validate dates
                if (request.StartDate >= request.EndDate)
                {
                    return Result<bool>.Failure("Start date must be before end date", "InvalidDateRange");
                }

                // Update using entity method (add this to entity)
                academicYear.Update(
                    request.Name,
                    request.StartDate,
                    request.EndDate,
                    _currentUserService.Username
                );

                _unitOfWork.AcademicYearRepository.UpdateAsync(academicYear);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Academic year updated: {AcademicYearId} by {User}",
                    academicYear.Id, _currentUserService.Username);

                return Result<bool>.Success(true, "Academic year updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating academic year {Id}", request.Id);
                return Result<bool>.Failure("An error occurred while updating the academic year", "UpdateFailed");
            }
        }
    }
}
