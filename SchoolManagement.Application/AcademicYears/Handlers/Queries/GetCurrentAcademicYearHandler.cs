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
    public class GetCurrentAcademicYearHandler : IRequestHandler<GetCurrentAcademicYearQuery, Result<AcademicYearDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCurrentAcademicYearHandler> _logger;

        public GetCurrentAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCurrentAcademicYearHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<AcademicYearDto>> Handle(GetCurrentAcademicYearQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currentYear = await _unitOfWork.AcademicYearRepository.GetCurrentAcademicYearAsync(cancellationToken);

                if (currentYear == null)
                {
                    return Result<AcademicYearDto>.Failure("No current academic year set", "NoCurrentYear");
                }

                var dto = new AcademicYearDto
                {
                    Id = currentYear.Id,
                    Name = currentYear.Name,
                    StartYear = currentYear.StartYear,
                    EndYear = currentYear.EndYear,
                    StartDate = currentYear.StartDate,
                    EndDate = currentYear.EndDate,
                    IsActive = currentYear.IsActive,
                    IsCurrent = currentYear.IsCurrent,
                    CreatedAt = currentYear.CreatedAt,
                    CreatedBy = currentYear.CreatedBy,
                    UpdatedAt = currentYear.UpdatedAt,
                    UpdatedBy = currentYear.UpdatedBy
                };

                return Result<AcademicYearDto>.Success(dto, "Current academic year retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current academic year");
                return Result<AcademicYearDto>.Failure("An error occurred while retrieving current academic year", "RetrieveFailed");
            }
        }
    }
}
