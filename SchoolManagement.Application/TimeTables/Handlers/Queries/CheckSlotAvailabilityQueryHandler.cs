using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Queries
{
    public class CheckSlotAvailabilityQueryHandler
        : IRequestHandler<CheckSlotAvailabilityQuery, Result<SlotAvailabilityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckSlotAvailabilityQueryHandler> _logger;

        public CheckSlotAvailabilityQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<CheckSlotAvailabilityQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<SlotAvailabilityDto>> Handle(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Checking slot availability for Section {SectionId}, Teacher {TeacherId}, Room {RoomNumber} on {DayOfWeek} Period {PeriodNumber}",
                    request.SectionId, request.TeacherId, request.RoomNumber,
                    request.DayOfWeek, request.PeriodNumber);

                // Validate inputs
                await ValidateRequestAsync(request, cancellationToken);

                // Check all conflicts in parallel for better performance
                var (sectionEntry, teacherEntry, roomEntry) = await CheckAllConflictsAsync(
                    request, cancellationToken);

                // Build detailed availability response
                var dto = BuildAvailabilityDto(
                    sectionEntry,
                    teacherEntry,
                    roomEntry,
                    request);

                var message = dto.IsAvailable
                    ? "Time slot is available for scheduling"
                    : BuildConflictMessage(dto);

                _logger.LogInformation(
                    "Slot availability check completed. Available: {IsAvailable}, Conflicts: Section={SectionConflict}, Teacher={TeacherConflict}, Room={RoomConflict}",
                    dto.IsAvailable,
                    dto.SectionConflict?.HasConflict ?? false,
                    dto.TeacherConflict?.HasConflict ?? false,
                    dto.RoomConflict?.HasConflict ?? false);

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
                    "Unexpected error occurred while checking slot availability for Section {SectionId}, Teacher {TeacherId}",
                    request.SectionId, request.TeacherId);

                return Result<SlotAvailabilityDto>.Failure(
                    "Failed to check slot availability.");
            }
        }

        private async Task ValidateRequestAsync(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            // Validate section exists
            var section = await _unitOfWork.SectionsRepository
                .GetByIdAsync(request.SectionId, cancellationToken);

            if (section == null)
            {
                throw new InvalidSectionException(
                    $"Section with ID {request.SectionId} not found");
            }

            // Validate teacher exists
            var teacher = await _unitOfWork.TeachersRepository
                .GetByIdAsync(request.TeacherId, cancellationToken);

            if (teacher == null)
            {
                throw new InvalidTimeTableEntryException(
                    $"Teacher with ID {request.TeacherId} not found");
            }

            // Validate day of week
            if (!Enum.IsDefined(typeof(DayOfWeek), request.DayOfWeek))
            {
                throw new InvalidDayOfWeekException(
                    $"Invalid day of week: {request.DayOfWeek}");
            }

            if (request.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new InvalidDayOfWeekException(
                    "Cannot schedule classes on Sunday");
            }

            // Validate period number
            if (request.PeriodNumber <= 0 || request.PeriodNumber > 10)
            {
                throw new InvalidPeriodNumberException(request.PeriodNumber, 10);
            }
        }

        private async Task<(Domain.Entities.TimeTableEntry sectionEntry,
                           Domain.Entities.TimeTableEntry teacherEntry,
                           Domain.Entities.TimeTableEntry roomEntry)> CheckAllConflictsAsync(
            CheckSlotAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            // Execute all checks in parallel for better performance
            var sectionTask = _unitOfWork.TimeTablesRepository.GetBySlotAsync(
                request.SectionId,
                request.DayOfWeek,
                request.PeriodNumber,
                cancellationToken);

            var teacherTask = _unitOfWork.TimeTablesRepository.GetTeacherScheduleAsync(
                request.TeacherId,
                request.DayOfWeek,
                request.PeriodNumber,
                cancellationToken);

            var roomTask = string.IsNullOrWhiteSpace(request.RoomNumber)
                ? Task.FromResult<Domain.Entities.TimeTableEntry>(null)
                : _unitOfWork.TimeTablesRepository.GetByRoomAndSlotAsync(
                    request.RoomNumber,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken);

            await Task.WhenAll(sectionTask, teacherTask, roomTask);

            return (await sectionTask, await teacherTask, await roomTask);
        }

        private SlotAvailabilityDto BuildAvailabilityDto(
            Domain.Entities.TimeTableEntry sectionEntry,
            Domain.Entities.TimeTableEntry teacherEntry,
            Domain.Entities.TimeTableEntry roomEntry,
            CheckSlotAvailabilityQuery request)
        {
            var dto = new SlotAvailabilityDto
            {
                SectionId = request.SectionId,
                TeacherId = request.TeacherId,
                RoomNumber = request.RoomNumber,
                DayOfWeek = request.DayOfWeek.ToString(),
                PeriodNumber = request.PeriodNumber,
                CheckedAt = DateTime.UtcNow
            };

            // Check section conflict
            if (sectionEntry != null)
            {
                dto.SectionConflict = new ConflictInfo
                {
                    HasConflict = true,
                    ConflictingEntryId = sectionEntry.Id,
                    ConflictType = "Section",
                    ConflictDetails = $"Section already has '{GetSubjectName(sectionEntry.SubjectId)}' scheduled with teacher {sectionEntry.TeacherId}",
                    ConflictingTeacherId = sectionEntry.TeacherId,
                    ConflictingSubjectId = sectionEntry.SubjectId,
                    ConflictingRoomNumber = sectionEntry.RoomNumber.Value,
                    TimeSlot = $"{sectionEntry.TimePeriod.StartTime:hh\\:mm} - {sectionEntry.TimePeriod.EndTime:hh\\:mm}"
                };
            }

            // Check teacher conflict
            if (teacherEntry != null)
            {
                dto.TeacherConflict = new ConflictInfo
                {
                    HasConflict = true,
                    ConflictingEntryId = teacherEntry.Id,
                    ConflictType = "Teacher",
                    ConflictDetails = $"Teacher is already teaching Section '{GetSectionName(teacherEntry.SectionId)}' in Room {teacherEntry.RoomNumber.Value}",
                    ConflictingSectionId = teacherEntry.SectionId,
                    ConflictingSubjectId = teacherEntry.SubjectId,
                    ConflictingRoomNumber = teacherEntry.RoomNumber.Value,
                    TimeSlot = $"{teacherEntry.TimePeriod.StartTime:hh\\:mm} - {teacherEntry.TimePeriod.EndTime:hh\\:mm}"
                };
            }

            // Check room conflict
            if (roomEntry != null)
            {
                dto.RoomConflict = new ConflictInfo
                {
                    HasConflict = true,
                    ConflictingEntryId = roomEntry.Id,
                    ConflictType = "Room",
                    ConflictDetails = $"Room {request.RoomNumber} is already booked for Section '{GetSectionName(roomEntry.SectionId)}'",
                    ConflictingSectionId = roomEntry.SectionId,
                    ConflictingTeacherId = roomEntry.TeacherId,
                    ConflictingSubjectId = roomEntry.SubjectId,
                    TimeSlot = $"{roomEntry.TimePeriod.StartTime:hh\\:mm} - {roomEntry.TimePeriod.EndTime:hh\\:mm}"
                };
            }

            // Determine overall availability
            dto.IsAvailable = dto.SectionConflict == null &&
                            dto.TeacherConflict == null &&
                            dto.RoomConflict == null;

            // Build conflicts summary
            dto.Conflicts = BuildConflictsSummary(dto);

            return dto;
        }

        private List<string> BuildConflictsSummary(SlotAvailabilityDto dto)
        {
            var conflicts = new List<string>();

            if (dto.SectionConflict?.HasConflict == true)
                conflicts.Add($"Section: {dto.SectionConflict.ConflictDetails}");

            if (dto.TeacherConflict?.HasConflict == true)
                conflicts.Add($"Teacher: {dto.TeacherConflict.ConflictDetails}");

            if (dto.RoomConflict?.HasConflict == true)
                conflicts.Add($"Room: {dto.RoomConflict.ConflictDetails}");

            return conflicts;
        }

        private string BuildConflictMessage(SlotAvailabilityDto dto)
        {
            if (dto.IsAvailable)
                return "Time slot is available for scheduling";

            var conflictTypes = new List<string>();

            if (dto.SectionConflict?.HasConflict == true)
                conflictTypes.Add("section");

            if (dto.TeacherConflict?.HasConflict == true)
                conflictTypes.Add("teacher");

            if (dto.RoomConflict?.HasConflict == true)
                conflictTypes.Add("room");

            return $"Time slot has conflicts: {string.Join(", ", conflictTypes)}";
        }

        private string GetSubjectName(Guid subjectId)
        {
            // TODO: Fetch from repository or cache
            return "Subject";
        }

        private string GetSectionName(Guid sectionId)
        {
            // TODO: Fetch from repository or cache
            return "Section";
        }
    }
}
