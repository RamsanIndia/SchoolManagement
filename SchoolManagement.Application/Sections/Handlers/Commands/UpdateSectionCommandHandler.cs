using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Sections.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class UpdateSectionCommandHandler
        : IRequestHandler<UpdateSectionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSectionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            var section = await _unitOfWork.SectionsRepository.GetByIdAsync(request.Id, cancellationToken);

            if (section == null)
            {
                return Result.Failure(
                    "Section not found.",
                    $"No section exists with Id: {request.Id}"
                );
            }

            // Domain behavior
            section.UpdateDetails(
                request.SectionName,
                request.Capacity,
                request.RoomNumber
            );

            await _unitOfWork.SectionsRepository.UpdateAsync(section, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Section updated successfully.");
        }
    }
}
