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
    public class DeleteAcademicYearHandler : IRequestHandler<DeleteAcademicYearCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteAcademicYearHandler> _logger;

        public DeleteAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteAcademicYearHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteAcademicYearCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYear = await _unitOfWork.AcademicYearRepository.GetByIdWithClassesAsync(request.Id, cancellationToken);

                if (academicYear == null || academicYear.IsDeleted)
                {
                    return Result<bool>.Failure("Academic year not found", "AcademicYearNotFound");
                }

                if (academicYear.IsCurrent)
                {
                    return Result<bool>.Failure("Cannot delete the current academic year", "CannotDeleteCurrent");
                }

                // Check if there are associated classes
                if (academicYear.Classes.Any())
                {
                    return Result<bool>.Failure("Cannot delete academic year with associated classes", "HasAssociatedClasses");
                }

                // Soft delete
                academicYear.MarkAsDeleted(_currentUserService.Username);
                _unitOfWork.AcademicYearRepository.UpdateAsync(academicYear);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Academic year deleted: {AcademicYearId} by {User}",
                    academicYear.Id, _currentUserService.Username);

                return Result<bool>.Success(true, "Academic year deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting academic year {Id}", request.Id);
                return Result<bool>.Failure("An error occurred while deleting the academic year", "DeleteFailed");
            }
        }
    }
}
