using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.AcademicYears.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Handlers.Commands
{
    public class CreateAcademicYearHandler : IRequestHandler<CreateAcademicYearCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateAcademicYearHandler> _logger;
        private readonly IpAddressHelper _ipAddressHelper;


        public CreateAcademicYearHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<CreateAcademicYearHandler> logger,
            IpAddressHelper ipAddressHelper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
            _ipAddressHelper = ipAddressHelper;
        }

        public async Task<Result<Guid>> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if academic year with same name exists
                var exists = await _unitOfWork.AcademicYearRepository.ExistsAsync(request.Name, null, cancellationToken);

                if (exists)
                {
                    return Result<Guid>.Failure("Academic year with this name already exists", "AcademicYearExists");
                }

                // Validate year range
                if (request.EndYear != request.StartYear + 1)
                {
                    return Result<Guid>.Failure("End year must be one year after start year", "InvalidYearRange");
                }

                // Validate dates
                if (request.StartDate >= request.EndDate)
                {
                    return Result<Guid>.Failure("Start date must be before end date", "InvalidDateRange");
                }

                // Create academic year
                var academicYear = AcademicYear.Create(
                    request.Name,
                    request.StartYear,
                    request.EndYear,
                    request.StartDate,
                    request.EndDate
                );

                academicYear.SetCreated(_currentUserService.Username, _ipAddressHelper.GetIpAddress());

                await _unitOfWork.AcademicYearRepository.AddAsync(academicYear, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Academic year created: {AcademicYearId} by {User}",
                    academicYear.Id, _currentUserService.Username);

                return Result<Guid>.Success(academicYear.Id, "Academic year created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating academic year");
                return Result<Guid>.Failure("An error occurred while creating the academic year", "CreateFailed");
            }
        }
    }
}
