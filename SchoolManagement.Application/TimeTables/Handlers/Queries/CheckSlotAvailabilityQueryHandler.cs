// Application/TimeTables/Handlers/Queries/CheckSlotAvailabilityQueryHandler.cs
using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Services;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Application.TimeTables.Validators;
using SchoolManagement.Application.TimeTables.Mappers;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Services;
using SchoolManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Queries
{
    /// <summary>
    /// Query handler for checking time slot availability
    /// Orchestrates validation, application service, domain events, and mapping
    /// Uses Unit of Work for all data access through Application Service
    /// </summary>
    public sealed class CheckSlotAvailabilityQueryHandler
        : IRequestHandler<CheckSlotAvailabilityQuery, Result<SlotAvailabilityDto>>
    {
        private readonly ISlotAvailabilityApplicationService _applicationService;
        private readonly ISlotAvailabilityValidator _validator;
        private readonly ISlotAvailabilityMapper _mapper;
        private readonly ILogger<CheckSlotAvailabilityQueryHandler> _logger;

        public CheckSlotAvailabilityQueryHandler(
            ISlotAvailabilityApplicationService applicationService,
            ISlotAvailabilityValidator validator,
            ISlotAvailabilityMapper mapper,
            ILogger<CheckSlotAvailabilityQueryHandler> logger)
        {
            _applicationService = applicationService
                ?? throw new ArgumentNullException(nameof(applicationService));
            _validator = validator
                ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<SlotAvailabilityDto>> Handle(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Checking slot availability for Section {SectionId}, Teacher {TeacherId}, " +
                    "Room {RoomNumber} on {DayOfWeek} Period {PeriodNumber}",
                    request.SectionId,
                    request.TeacherId,
                    request.RoomNumber,
                    request.DayOfWeek,
                    request.PeriodNumber);

                // Step 1: Validate the request (uses Unit of Work internally)
                await _validator.ValidateAsync(request, cancellationToken);

                // Step 2: Create domain request object
                var availabilityRequest = new SlotAvailabilityRequest(
                    request.SectionId,
                    request.TeacherId,
                    request.RoomNumber,
                    request.DayOfWeek,
                    request.PeriodNumber);

                // Step 3: Check availability using Application Service 
                // (handles Unit of Work and Domain Service)
                var availabilityResult = await _applicationService
                    .CheckAvailabilityAsync(availabilityRequest, cancellationToken);

                // Step 4: Map domain result to DTO
                var dto = _mapper.MapToDto(request, availabilityResult);

                // Step 5: Build response message
                var message = BuildResponseMessage(availabilityResult);

                _logger.LogInformation(
                    "Slot availability check completed. Available: {IsAvailable}, " +
                    "Conflicts: {ConflictCount}",
                    availabilityResult.IsAvailable,
                    availabilityResult.Conflicts.Count);

                return Result<SlotAvailabilityDto>.Success(dto, message);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while checking slot availability: {Message}",
                    ex.Message);

                return Result<SlotAvailabilityDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while checking slot availability for " +
                    "Section {SectionId}, Teacher {TeacherId}",
                    request.SectionId,
                    request.TeacherId);

                return Result<SlotAvailabilityDto>.Failure(
                    "Failed to check slot availability.");
            }
        }

        private static string BuildResponseMessage(SlotAvailabilityResult result)
        {
            if (result.IsAvailable)
            {
                return "Time slot is available for scheduling";
            }

            var conflictTypes = result.Conflicts
                .Select(c => c.Type.ToString().ToLower())
                .Distinct();

            return $"Time slot has conflicts: {string.Join(", ", conflictTypes)}";
        }
    }
}
