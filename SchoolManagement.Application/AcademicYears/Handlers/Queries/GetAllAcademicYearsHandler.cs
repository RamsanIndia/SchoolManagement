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
    public class GetAllAcademicYearsHandler : IRequestHandler<GetAllAcademicYearsQuery, Result<List<AcademicYearDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllAcademicYearsHandler> _logger;

        public GetAllAcademicYearsHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetAllAcademicYearsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<List<AcademicYearDto>>> Handle(GetAllAcademicYearsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var academicYears = request.ActiveOnly
                    ? await _unitOfWork.AcademicYearRepository.GetAllActiveAsync(cancellationToken)
                    : await _unitOfWork.AcademicYearRepository.GetAllAsync(cancellationToken);

                var dtos = academicYears
                    .Where(ay => !ay.IsDeleted)
                    .OrderByDescending(ay => ay.StartYear)
                    .Select(ay => new AcademicYearDto
                    {
                        Id = ay.Id,
                        Name = ay.Name,
                        StartYear = ay.StartYear,
                        EndYear = ay.EndYear,
                        StartDate = ay.StartDate,
                        EndDate = ay.EndDate,
                        IsActive = ay.IsActive,
                        IsCurrent = ay.IsCurrent,
                        CreatedAt = ay.CreatedAt,
                        CreatedBy = ay.CreatedBy,
                        UpdatedAt = ay.UpdatedAt,
                        UpdatedBy = ay.UpdatedBy
                    })
                    .ToList();

                return Result<List<AcademicYearDto>>.Success(dtos, $"{dtos.Count} academic year(s) retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving academic years");
                return Result<List<AcademicYearDto>>.Failure("An error occurred while retrieving academic years", "RetrieveFailed");
            }
        }
    }
}
