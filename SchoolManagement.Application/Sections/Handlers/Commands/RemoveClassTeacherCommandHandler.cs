using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Sections.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class RemoveClassTeacherCommandHandler
        : IRequestHandler<RemoveClassTeacherCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveClassTeacherCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RemoveClassTeacherCommand request, CancellationToken cancellationToken)
        {
            var section = await _unitOfWork.SectionsRepository.GetByIdAsync(request.SectionId, cancellationToken);

            if (section == null)
            {
                return Result.Failure(
                    "Section not found.",
                    $"No section exists with Id: {request.SectionId}"
                );
            }

            // Domain logic
            section.RemoveClassTeacher();

            await _unitOfWork.SectionsRepository.UpdateAsync(section, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Class teacher removed successfully.");
        }
    }
}
