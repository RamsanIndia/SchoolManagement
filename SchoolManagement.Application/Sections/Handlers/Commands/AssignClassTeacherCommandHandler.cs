using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Sections.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class AssignClassTeacherCommandHandler
        : IRequestHandler<AssignClassTeacherCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignClassTeacherCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(AssignClassTeacherCommand request, CancellationToken cancellationToken)
        {
            var section = await _unitOfWork.SectionsRepository
                .GetByIdAsync(request.SectionId, cancellationToken);

            if (section == null)
            {
                return Result<bool>.Failure(
                    "Section not found",
                    $"No section exists with Id: {request.SectionId}"
                );
            }

            section.AssignClassTeacher(request.TeacherId);

            await _unitOfWork.SectionsRepository.UpdateAsync(section, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Class teacher assigned successfully.");
        }
    }
}
