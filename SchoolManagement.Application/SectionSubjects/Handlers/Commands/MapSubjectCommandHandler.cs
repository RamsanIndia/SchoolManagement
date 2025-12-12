using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.SectionSubjects.Commands;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Handlers.Commands
{
    public class MapSubjectCommandHandler : IRequestHandler<MapSubjectCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MapSubjectCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(MapSubjectCommand request, CancellationToken cancellationToken)
        {
            // Check duplicate mapping
            var exists = await _unitOfWork.SectionSubjectsRepository.IsSubjectMappedAsync(
                request.SectionId,
                request.SubjectId,
                cancellationToken
            );

            if (exists)
            {
                return Result<Guid>.Failure(
                    "Subject already mapped",
                    $"The subject '{request.SubjectName}' is already mapped to this section."
                );
            }

            var sectionSubject = new SectionSubject(
                request.SectionId,
                request.SubjectId,
                request.SubjectName,
                request.SubjectCode,
                request.TeacherId,
                request.TeacherName,
                request.WeeklyPeriods,
                request.IsMandatory
            );

            await _unitOfWork.SectionSubjectsRepository.AddAsync(sectionSubject, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(
                sectionSubject.Id,
                "Subject mapped to section successfully."
            );
        }
    }
}
