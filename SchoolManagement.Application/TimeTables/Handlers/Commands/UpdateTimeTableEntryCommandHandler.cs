using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class UpdateTimeTableEntryCommandHandler
        : IRequestHandler<UpdateTimeTableEntryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTimeTableEntryCommandHandler> _logger;
        private readonly IMediator _mediator;

        public UpdateTimeTableEntryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateTimeTableEntryCommandHandler> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result> Handle(
            UpdateTimeTableEntryCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Updating TimeTable entry {EntryId}",
                    request.Id);

                // Get existing entry
                var entry = await _unitOfWork.TimeTablesRepository
                    .GetByIdAsync(request.Id, cancellationToken);

                if (entry == null)
                {
                    _logger.LogWarning(
                        "TimeTable entry {EntryId} not found",
                        request.Id);

                    return Result.Failure(
                        $"TimeTable entry with ID {request.Id} not found");
                }

                // Validate business rules (conflicts with other entries)
                await ValidateBusinessRulesAsync(entry, request, cancellationToken);

                // Update using domain method - this will validate and raise domain events
                entry.UpdateSchedule(
                    request.SubjectId,
                    request.TeacherId,
                    request.StartTime,
                    request.EndTime,
                    request.RoomNumber
                );

                // Persist changes
                _unitOfWork.TimeTablesRepository.UpdateAsync(entry);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Dispatch domain events
                await DispatchDomainEventsAsync(entry, cancellationToken);

                _logger.LogInformation(
                    "TimeTable entry {EntryId} updated successfully",
                    entry.Id);

                return Result.Success("Time table entry updated successfully.");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while updating TimeTable entry {EntryId}: {Message}",
                    request.Id, ex.Message);

                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while updating TimeTable entry {EntryId}",
                    request.Id);

                return Result.Failure("Failed to update time table entry.");
            }
        }

        private async Task ValidateBusinessRulesAsync(
            Domain.Entities.TimeTableEntry currentEntry,
            UpdateTimeTableEntryCommand request,
            CancellationToken cancellationToken)
        {
            // Check if teacher is available at this time (excluding current entry)
            var teacherEntry = await _unitOfWork.TimeTablesRepository
                .GetTeacherScheduleAsync(
                    request.TeacherId,
                    currentEntry.DayOfWeek,
                    currentEntry.PeriodNumber,
                    cancellationToken);

            if (teacherEntry != null && teacherEntry.Id != request.Id)
            {
                throw new TimeTableConflictException(
                    $"Teacher is already assigned to Section {teacherEntry.SectionId} at {currentEntry.DayOfWeek}, Period {currentEntry.PeriodNumber}",
                    teacherEntry.Id,
                    currentEntry.DayOfWeek,
                    currentEntry.PeriodNumber);
            }

            // Check if room is available (excluding current entry)
            var roomEntry = await _unitOfWork.TimeTablesRepository
                .GetByRoomAndSlotAsync(
                    request.RoomNumber,
                    currentEntry.DayOfWeek,
                    currentEntry.PeriodNumber,
                    cancellationToken);

            if (roomEntry != null && roomEntry.Id != request.Id)
            {
                throw new TimeTableConflictException(
                    $"Room {request.RoomNumber} is already booked for Section {roomEntry.SectionId} at {currentEntry.DayOfWeek}, Period {currentEntry.PeriodNumber}",
                    roomEntry.Id,
                    currentEntry.DayOfWeek,
                    currentEntry.PeriodNumber);
            }
        }

        private async Task DispatchDomainEventsAsync(
            Domain.Entities.TimeTableEntry entry,
            CancellationToken cancellationToken)
        {
            var events = entry.DomainEvents;
            entry.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
