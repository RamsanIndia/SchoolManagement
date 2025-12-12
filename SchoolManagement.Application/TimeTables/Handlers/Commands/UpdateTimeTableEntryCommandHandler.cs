using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.TimeTables.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class UpdateTimeTableEntryCommandHandler
        : IRequestHandler<UpdateTimeTableEntryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTimeTableEntryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateTimeTableEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entry = await _unitOfWork.TimeTablesRepository.GetByIdAsync(request.Id, cancellationToken);

                if (entry == null)
                    return Result.Failure("Timetable entry not found.", $"No entry exists with Id: {request.Id}");

                // Update domain entity
                entry.UpdateDetails(
                    request.SubjectId,
                    request.TeacherId,
                    request.StartTime,
                    request.EndTime,
                    request.RoomNumber
                );

                await _unitOfWork.TimeTablesRepository.UpdateAsync(entry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Timetable entry updated successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to update timetable entry.", ex.Message);
            }
        }
    }
}
