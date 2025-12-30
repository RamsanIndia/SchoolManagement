using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.AcademicYears.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Handlers.Queries
{
    public class GetAcademicYearByIdHandler : IRequestHandler<GetAcademicYearByIdQuery, Result<AcademicYearDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAcademicYearByIdHandler> _logger;

        public GetAcademicYearByIdHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetAcademicYearByIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<AcademicYearDto>> Handle(GetAcademicYearByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYear = await _unitOfWork.AcademicYearRepository.GetByIdAsync(request.Id, cancellationToken);

                if (academicYear == null || academicYear.IsDeleted)
                {
                    return Result<AcademicYearDto>.Failure("Academic year not found", "AcademicYearNotFound");
                }

                var dto = new AcademicYearDto
                {
                    Id = academicYear.Id,
                    Name = academicYear.Name,
                    StartYear = academicYear.StartYear,
                    EndYear = academicYear.EndYear,
                    StartDate = academicYear.StartDate,
                    EndDate = academicYear.EndDate,
                    IsActive = academicYear.IsActive,
                    IsCurrent = academicYear.IsCurrent,
                    CreatedAt = academicYear.CreatedAt,
                    CreatedBy = academicYear.CreatedBy,
                    UpdatedAt = academicYear.UpdatedAt,
                    UpdatedBy = academicYear.UpdatedBy
                };

                return Result<AcademicYearDto>.Success(dto, "Academic year retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving academic year {Id}", request.Id);
                return Result<AcademicYearDto>.Failure("An error occurred while retrieving the academic year", "RetrieveFailed");
            }
        }
    }
}
