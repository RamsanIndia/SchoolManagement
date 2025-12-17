using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class CreateTimeTableEntryCommandHandler
        : IRequestHandler<CreateTimeTableEntryCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateTimeTableEntryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateTimeTableEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if slot is available
                var slotAvailable = await _unitOfWork.TimeTablesRepository.IsSlotAvailableAsync(
                    request.SectionId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken
                );

                if (!slotAvailable)
                    return Result<Guid>.Failure("This slot is already occupied.");

                // Check if teacher is available
                var teacherAvailable = await _unitOfWork.TimeTablesRepository.IsTeacherAvailableAsync(
                    request.TeacherId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    cancellationToken
                );

                if (!teacherAvailable)
                    return Result<Guid>.Failure("Teacher is not available at this time.");

                // Create new TimeTableEntry
                var entry = new TimeTableEntry(
                    request.SectionId,
                    request.SubjectId,
                    request.TeacherId,
                    request.DayOfWeek,
                    request.PeriodNumber,
                    request.StartTime,
                    request.EndTime,
                    request.RoomNumber
                );

                await _unitOfWork.TimeTablesRepository.AddAsync(entry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(entry.Id, "Time table entry created successfully.");
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure("Failed to create time table entry.", ex.Message);
            }
        }
    }
}
