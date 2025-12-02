using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.SectionSubjects.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Handlers.Commands
{
    public class UpdateSubjectMappingCommandHandler
        : IRequestHandler<UpdateSubjectMappingCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSubjectMappingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(UpdateSubjectMappingCommand request, CancellationToken cancellationToken)
        {
            var mapping = await _unitOfWork.SectionSubjectsRepository
                .GetByIdAsync(request.MappingId, cancellationToken);

            if (mapping == null)
            {
                return Result<bool>.Failure(
                    "Subject mapping not found",
                    $"No mapping exists with Id: {request.MappingId}"
                );
            }

            // Update domain entity
            mapping.UpdateTeacher(request.TeacherId, request.TeacherName);
            mapping.UpdateWeeklyPeriods(request.WeeklyPeriods);

            await _unitOfWork.SectionSubjectsRepository.UpdateAsync(mapping, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Subject mapping updated successfully.");
        }
    }
}
