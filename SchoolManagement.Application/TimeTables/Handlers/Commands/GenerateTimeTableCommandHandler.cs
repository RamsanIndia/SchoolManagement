using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class GenerateTimeTableCommandHandler
        : IRequestHandler<GenerateTimeTableCommand, Result<TimeTableGenerationResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeTableGenerationService _timeTableGenerationService;
        private readonly ILogger<GenerateTimeTableCommandHandler> _logger;
        private readonly IMediator _mediator;

        public GenerateTimeTableCommandHandler(
            IUnitOfWork unitOfWork,
            ITimeTableGenerationService timeTableGenerationService,
            ILogger<GenerateTimeTableCommandHandler> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _timeTableGenerationService = timeTableGenerationService;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<TimeTableGenerationResultDto>> Handle(
            GenerateTimeTableCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Starting timetable generation for Section {SectionId} with {PeriodsPerDay} periods per day",
                    request.SectionId, request.PeriodsPerDay);

                // Validate prerequisites
                var validationResult = await ValidatePrerequisitesAsync(request, cancellationToken);
                if (!validationResult.Status)
                {
                    return Result<TimeTableGenerationResultDto>.Failure(validationResult.Message);
                }

                // Get section with details
                var section = await _unitOfWork.SectionsRepository
                    .GetByIdWithDetailsAsync(request.SectionId, cancellationToken);

                // Get subjects for the section
                var subjects = await _unitOfWork.SectionSubjectsRepository
                    .GetBySectionIdAsync(request.SectionId, cancellationToken);

                // Get existing timetable entries
                var existingEntries = await _unitOfWork.TimeTablesRepository
                    .GetBySectionIdAsync(request.SectionId, cancellationToken);

                // Handle overwrite scenario
                if (request.OverwriteExisting && existingEntries.Any())
                {
                    _logger.LogInformation(
                        "Overwriting {Count} existing timetable entries for Section {SectionId}",
                        existingEntries.Count(), request.SectionId);

                    await DeleteExistingEntriesAsync(existingEntries, cancellationToken);
                    existingEntries = Enumerable.Empty<TimeTableEntry>();
                }

                // Build options object
                var options = new TimeTableGenerationOptions
                {
                    PeriodsPerDay = request.PeriodsPerDay,
                    PeriodDuration = request.PeriodDuration,
                    BreakAfterPeriod = request.BreakAfterPeriod,
                    BreakDuration = request.BreakDuration,
                    SchoolStartTime = request.SchoolStartTime,
                    OverwriteExisting = request.OverwriteExisting,
                    WorkingDays = request.WorkingDays
                };

                // Generate timetable using domain service
                var generationResult = _timeTableGenerationService.GenerateTimeTable(
                    section,
                    subjects.ToList(),
                    existingEntries.ToList(),
                    options
                );

                // Persist new entries
                foreach (var entry in generationResult.NewEntries)
                {
                    await _unitOfWork.TimeTablesRepository.AddAsync(entry, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Dispatch domain events
                foreach (var entry in generationResult.NewEntries)
                {
                    await DispatchDomainEventsAsync(entry, cancellationToken);
                }

                _logger.LogInformation(
                    "Successfully generated {Count} timetable entries for Section {SectionId}",
                    generationResult.EntriesCreated,
                    request.SectionId);

                // Map to DTO
                var response = generationResult.ToDto(request.SectionId);
                var message = BuildSuccessMessage(generationResult);

                return Result<TimeTableGenerationResultDto>.Success(response, message);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while generating timetable for Section {SectionId}: {Message}",
                    request.SectionId, ex.Message);

                return Result<TimeTableGenerationResultDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while generating timetable for Section {SectionId}",
                    request.SectionId);

                return Result<TimeTableGenerationResultDto>.Failure(
                    "Failed to generate timetable.");
            }
        }

        private async Task<Result> ValidatePrerequisitesAsync(
            GenerateTimeTableCommand request,
            CancellationToken cancellationToken)
        {
            var section = await _unitOfWork.SectionsRepository
                .GetByIdAsync(request.SectionId, cancellationToken);

            if (section == null)
            {
                return Result.Failure($"Section with ID {request.SectionId} not found.");
            }

            var subjects = await _unitOfWork.SectionSubjectsRepository
                .GetBySectionIdAsync(request.SectionId, cancellationToken);

            if (!subjects.Any())
            {
                return Result.Failure(
                    $"No subjects mapped to section {request.SectionId}. Please assign subjects before generating timetable.");
            }

            var subjectsWithoutTeachers = subjects.Where(s => s.TeacherId == Guid.Empty).ToList();
            if (subjectsWithoutTeachers.Any())
            {
                var subjectNames = string.Join(", ", subjectsWithoutTeachers.Select(s => s.SubjectName));
                return Result.Failure(
                    $"The following subjects do not have assigned teachers: {subjectNames}");
            }

            // Check for existing entries if not overwriting
            if (!request.OverwriteExisting)
            {
                var existingEntries = await _unitOfWork.TimeTablesRepository
                    .GetBySectionIdAsync(request.SectionId, cancellationToken);

                if (existingEntries.Any())
                {
                    return Result.Failure(
                        $"Section {request.SectionId} already has {existingEntries.Count()} timetable entries. " +
                        "Set OverwriteExisting to true to replace them.");
                }
            }

            return Result.Success();
        }

        private async Task DeleteExistingEntriesAsync(
            IEnumerable<TimeTableEntry> entries,
            CancellationToken cancellationToken)
        {
            foreach (var entry in entries)
            {
                entry.Cancel(); // Soft delete using domain method
                _unitOfWork.TimeTablesRepository.UpdateAsync(entry);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(
            TimeTableEntry entry,
            CancellationToken cancellationToken)
        {
            var events = entry.DomainEvents;
            entry.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }

        private string BuildSuccessMessage(TimeTableGenerationResult result)
        {
            var message = $"Successfully created {result.EntriesCreated} timetable entries.";

            if (result.EntriesSkipped > 0)
            {
                message += $" Skipped {result.EntriesSkipped} time slots that are already occupied.";
            }

            if (result.Warnings.Any())
            {
                message += $" Generated with {result.Warnings.Count} warnings.";
            }

            return message;
        }
    }
}
