using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.TimeTables.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class DeleteTimeTableEntryCommandHandler : IRequestHandler<DeleteTimeTableEntryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTimeTableEntryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteTimeTableEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entry = await _unitOfWork.TimeTablesRepository.GetByIdAsync(request.Id, cancellationToken);
                if (entry == null)
                    return Result.Failure("Timetable entry not found");

                await _unitOfWork.TimeTablesRepository.DeleteAsync(entry, cancellationToken);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success("Timetable entry deleted successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to delete timetable entry", ex.Message);
            }
        }
    }
}
