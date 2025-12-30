using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Sections.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Queries
{
    public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, Result<SectionDto>>
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetSectionByIdQueryHandler> _logger;

        public GetSectionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetSectionByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<SectionDto>> Handle(
            GetSectionByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving section {SectionId}", request.SectionId);

                var section = await _unitOfWork.SectionsRepository
                    .GetByIdWithDetailsAsync(request.SectionId, cancellationToken);

                if (section == null)
                {
                    _logger.LogWarning("Section {SectionId} not found", request.SectionId);
                    return Result<SectionDto>.Failure(
                        "Section not found.",
                        $"Section with ID '{request.SectionId}' does not exist.");
                }

                // Map to DTO
                var dto = new SectionDto
                {
                    Id = section.Id,
                    SectionName = section.Name,
                    ClassId = section.ClassId,
                    ClassName = section.Class?.Name,
                    MaxCapacity = section.Capacity.MaxCapacity,
                    CurrentStrength = section.Capacity.CurrentStrength,
                    AvailableSeats = section.Capacity.AvailableSeats(),
                    RoomNumber = section.RoomNumber.Value,
                    ClassTeacherId = section.ClassTeacherId,
                    IsActive = section.IsActive,
                    CreatedAt = section.CreatedAt,
                    CreatedBy = section.CreatedBy,
                    UpdatedBy = section.UpdatedBy
                };

                _logger.LogInformation("Successfully retrieved section {SectionId}", request.SectionId);

                return Result<SectionDto>.Success(dto, "Section retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving section {SectionId}", request.SectionId);
                return Result<SectionDto>.Failure(
                    "An error occurred while retrieving the section.",
                    ex.Message);
            }
        }
    }
    
}
