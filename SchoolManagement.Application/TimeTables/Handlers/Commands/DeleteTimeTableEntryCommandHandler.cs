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
    public class DeleteTimeTableEntryCommandHandler
        : IRequestHandler<DeleteTimeTableEntryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTimeTableEntryCommandHandler> _logger;
        private readonly IMediator _mediator;

        public DeleteTimeTableEntryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteTimeTableEntryCommandHandler> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result> Handle(
            DeleteTimeTableEntryCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting TimeTable entry {EntryId}",
                    request.Id);

                // Get existing entry
                var entry = await _unitOfWork.TimeTablesRepository
                    .GetByIdAsync(request.Id, cancellationToken);

                if (entry == null)
                {
                    _logger.LogWarning(
                        "TimeTable entry {EntryId} not found for deletion",
                        request.Id);

                    return Result.Failure(
                        $"Timetable entry with ID {request.Id} not found");
                }

                // Check if already deleted (soft delete check)
                if (entry.IsDeleted)
                {
                    _logger.LogWarning(
                        "TimeTable entry {EntryId} is already deleted",
                        request.Id);

                    return Result.Failure(
                        "Timetable entry has already been deleted");
                }

                // Use domain method for soft delete (maintains audit trail)
                entry.Cancel();

                // Update repository
                _unitOfWork.TimeTablesRepository.UpdateAsync(entry);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "TimeTable entry {EntryId} deleted successfully for Section {SectionId}, {DayOfWeek} Period {PeriodNumber}",
                    entry.Id, entry.SectionId, entry.DayOfWeek, entry.PeriodNumber);

                return Result.Success("Timetable entry deleted successfully");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while deleting TimeTable entry {EntryId}: {Message}",
                    request.Id, ex.Message);

                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while deleting TimeTable entry {EntryId}",
                    request.Id);

                return Result.Failure(
                    "Failed to delete timetable entry.");
            }
        }
    }
}
