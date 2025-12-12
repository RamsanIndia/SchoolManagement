using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.TimeTables.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Queries
{
    public class CheckSlotAvailabilityQueryHandler
        : IRequestHandler<CheckSlotAvailabilityQuery, Result<SlotAvailabilityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckSlotAvailabilityQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<SlotAvailabilityDto>> Handle(CheckSlotAvailabilityQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if section slot is available
                var sectionAvailable = await _unitOfWork.TimeTablesRepository.IsSlotAvailableAsync(
                    request.SectionId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken
                );

                // Check if teacher is available
                var teacherAvailable = await _unitOfWork.TimeTablesRepository.IsTeacherAvailableAsync(
                    request.TeacherId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken
                );

                // Build result DTO
                var slotDto = new SlotAvailabilityDto
                {
                    IsSectionSlotAvailable = sectionAvailable,
                    IsTeacherAvailable = teacherAvailable,
                    CanSchedule = sectionAvailable && teacherAvailable,
                    Message = sectionAvailable && teacherAvailable
                        ? "Slot is available for scheduling"
                        : !sectionAvailable
                            ? "Section already has a class scheduled at this time"
                            : "Teacher is not available at this time"
                };

                return Result<SlotAvailabilityDto>.Success(slotDto, "Slot availability checked successfully.");
            }
            catch (Exception ex)
            {
                return Result<SlotAvailabilityDto>.Failure("Failed to check slot availability.", ex.Message);
            }
        }
    }
}
