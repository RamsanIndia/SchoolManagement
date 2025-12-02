using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Sections.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class DeleteSectionCommandHandler
        : IRequestHandler<DeleteSectionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSectionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
        {
            var section = await _unitOfWork.SectionsRepository.GetByIdAsync(request.Id, cancellationToken);

            if (section == null)
            {
                return Result.Failure(
                    "Section not found.",
                    $"No section exists with Id: {request.Id}"
                );
            }

            await _unitOfWork.SectionsRepository.DeleteAsync(section, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Section deleted successfully.");
        }
    }
}
