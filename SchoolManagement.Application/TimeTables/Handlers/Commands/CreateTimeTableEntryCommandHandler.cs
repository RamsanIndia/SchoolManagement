using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class CreateTimeTableEntryCommandHandler
        : IRequestHandler<CreateTimeTableEntryCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateTimeTableEntryCommandHandler> _logger;
        private readonly IMediator _mediator;

        public CreateTimeTableEntryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreateTimeTableEntryCommandHandler> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<Guid>> Handle(
            CreateTimeTableEntryCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating TimeTable entry for Section {SectionId}, Subject {SubjectId}, Day {DayOfWeek}, Period {PeriodNumber}",
                    request.SectionId, request.SubjectId, request.DayOfWeek, request.PeriodNumber);

                // Validate business rules
                await ValidateBusinessRulesAsync(request, cancellationToken);

                // Create aggregate using factory method
                var entry = TimeTableEntry.Create(
                    request.SectionId,
                    request.SubjectId,
                    request.TeacherId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    request.StartTime,
                    request.EndTime,
                    request.RoomNumber
                );

                // Persist the aggregate
                await _unitOfWork.TimeTablesRepository.AddAsync(entry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Dispatch domain events
                await DispatchDomainEventsAsync(entry, cancellationToken);

                _logger.LogInformation(
                    "TimeTable entry {EntryId} created successfully",
                    entry.Id);

                return Result<Guid>.Success(
                    entry.Id,
                    "Time table entry created successfully.");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while creating TimeTable entry: {Message}",
                    ex.Message);

                return Result<Guid>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while creating TimeTable entry");

                return Result<Guid>.Failure(
                    "Failed to create time table entry.",
                    ex.Message);
            }
        }

        private async Task ValidateBusinessRulesAsync(
            CreateTimeTableEntryCommand request,
            CancellationToken cancellationToken)
        {
            // Check if section slot is available
            var sectionEntry = await _unitOfWork.TimeTablesRepository
                .GetBySlotAsync(
                    request.SectionId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken);

            if (sectionEntry != null)
            {
                throw new TimeTableConflictException(
                    sectionEntry.Id,
                    request.DayOfWeek,
                    request.PeriodNumber);
            }

            // Check if teacher is available at this time
            var teacherEntry = await _unitOfWork.TimeTablesRepository
                .GetTeacherScheduleAsync(
                    request.TeacherId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken);

            if (teacherEntry != null)
            {
                throw new TimeTableConflictException(
                    $"Teacher is already assigned to Section {teacherEntry.SectionId} at {request.DayOfWeek}, Period {request.PeriodNumber}",
                    teacherEntry.Id,
                    request.DayOfWeek,
                    request.PeriodNumber);
            }

            // Check if room is available
            var roomEntry = await _unitOfWork.TimeTablesRepository
                .GetByRoomAndSlotAsync(
                    request.RoomNumber,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken);

            if (roomEntry != null)
            {
                throw new TimeTableConflictException(
                    $"Room {request.RoomNumber} is already booked for Section {roomEntry.SectionId} at {request.DayOfWeek}, Period {request.PeriodNumber}",
                    roomEntry.Id,
                    request.DayOfWeek,
                    request.PeriodNumber);
            }
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
    }
}
