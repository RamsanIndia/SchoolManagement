using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Teachers.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Handlers.Commands
{
    public class DeactivateTeacherCommandHandler
        : IRequestHandler<DeactivateTeacherCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeactivateTeacherCommandHandler> _logger;

        public DeactivateTeacherCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeactivateTeacherCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeactivateTeacherCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var teacher = await _unitOfWork.TeachersRepository
                    .GetTeacherWithDetailsAsync(request.TeacherId, cancellationToken);

                if (teacher == null)
                {
                    return Result<bool>.Failure(
                        "TeacherNotFound",
                        $"Teacher with ID '{request.TeacherId}' not found"
                    );
                }

                teacher.Deactivate(_currentUserService.Username);

                await _unitOfWork.TeachersRepository.UpdateAsync(teacher, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Teacher deactivated. TeacherId: {TeacherId}",
                    teacher.Id
                );

                return Result<bool>.Success(
                    true,
                    $"Teacher '{teacher.FullName}' deactivated successfully"
                );
            }
            catch (TeacherHasActiveAssignmentsException ex)
            {
                _logger.LogWarning(ex, "Cannot deactivate teacher with active assignments");
                return Result<bool>.Failure("HasActiveAssignments", ex.Message);
            }
            catch (TeacherAlreadyInactiveException ex)
            {
                _logger.LogWarning(ex, "Teacher is already inactive");
                return Result<bool>.Failure("AlreadyInactive", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating teacher");
                return Result<bool>.Failure("UnexpectedError", "An error occurred");
            }
        }
    }
}
