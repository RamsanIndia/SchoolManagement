using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    /// <summary>
    /// Application service that orchestrates Unit of Work and Domain Service
    /// Handles all data access through Unit of Work pattern
    /// </summary>
    public sealed class SlotAvailabilityApplicationService
        : ISlotAvailabilityApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlotAvailabilityService _domainService;

        public SlotAvailabilityApplicationService(
            IUnitOfWork unitOfWork,
            ISlotAvailabilityService domainService)
        {
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
            _domainService = domainService
                ?? throw new ArgumentNullException(nameof(domainService));
        }

        public async Task<SlotAvailabilityResult> CheckAvailabilityAsync(
            SlotAvailabilityRequest request,
            CancellationToken cancellationToken = default)
        {
            // Fetch all conflicting entries through Unit of Work (sequential execution)
            var (sectionEntry, teacherEntry, roomEntry) =
                await FetchConflictingEntriesSequentiallyAsync(request, cancellationToken);

            // Delegate business logic to domain service (pure domain logic, no data access)
            var result = _domainService.CheckAvailability(
                request,
                sectionEntry,
                teacherEntry,
                roomEntry);

            return result;
        }

        /// <summary>
        /// Fetches conflicting entries SEQUENTIALLY to avoid DbContext concurrency issues.
        /// Even with AsNoTracking(), parallel queries on the same DbContext can cause errors.
        /// Sequential execution adds minimal overhead (~5-15ms) but ensures reliability.
        /// </summary>
        private async Task<(TimeTableEntry sectionEntry,
                           TimeTableEntry teacherEntry,
                           TimeTableEntry roomEntry)>
            FetchConflictingEntriesSequentiallyAsync(
                SlotAvailabilityRequest request,
                CancellationToken cancellationToken)
        {
            // Execute queries ONE AT A TIME to prevent DbContext threading issues

            // Query 1: Check if section already has a class scheduled
            var sectionEntry = await _unitOfWork.TimeTablesRepository.GetBySlotAsync(
                request.SectionId,
                request.DayOfWeek,
                request.PeriodNumber,
                cancellationToken);

            // Query 2: Check if teacher is already teaching another class
            var teacherEntry = await _unitOfWork.TimeTablesRepository.GetTeacherScheduleAsync(
                request.TeacherId,
                request.DayOfWeek,
                request.PeriodNumber,
                cancellationToken);

            // Query 3: Check if room is already occupied (only if room specified)
            TimeTableEntry roomEntry = null;
            if (!string.IsNullOrWhiteSpace(request.RoomNumber))
            {
                roomEntry = await _unitOfWork.TimeTablesRepository.GetByRoomAndSlotAsync(
                    request.RoomNumber,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken);
            }

            return (sectionEntry, teacherEntry, roomEntry);
        }
    }
}
