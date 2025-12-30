using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
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
        private readonly ICurrentUserService _currentUserService;

        public UpdateSubjectMappingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
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
            var userId = _currentUserService.Username ?? "System";
            // Update domain entity
            mapping.UpdateTeacher(request.TeacherId, request.TeacherName, userId);
            mapping.UpdateWeeklyPeriods(request.WeeklyPeriods, userId);

            await _unitOfWork.SectionSubjectsRepository.UpdateAsync(mapping, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Subject mapping updated successfully.");
        }
    }
}
