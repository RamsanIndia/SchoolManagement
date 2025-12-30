using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Sections.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class AssignClassTeacherCommandHandler
    : IRequestHandler<AssignClassTeacherCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AssignClassTeacherCommandHandler> _logger;

        public AssignClassTeacherCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<AssignClassTeacherCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            AssignClassTeacherCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get current user context
                var currentUserId = _currentUserService.Username;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Result<bool>.Failure(
                        "Unauthorized",
                        "Current user context is required to perform this operation"
                    );
                }

                // Retrieve section
                var section = await _unitOfWork.SectionsRepository
                    .GetByIdAsync(request.SectionId, cancellationToken);

                if (section == null)
                {
                    return Result<bool>.Failure(
                        "SectionNotFound",
                        $"No section exists with Id: {request.SectionId}"
                    );
                }

                // Verify teacher exists and is active
                var teacher = await _unitOfWork.TeachersRepository
                    .GetByIdAsync(request.TeacherId, cancellationToken);

                if (teacher == null)
                {
                    return Result<bool>.Failure(
                        "TeacherNotFound",
                        $"No teacher exists with Id: {request.TeacherId}"
                    );
                }

                if (!teacher.IsActive)
                {
                    return Result<bool>.Failure(
                        "TeacherInactive",
                        "Cannot assign an inactive teacher as class teacher"
                    );
                }

                // Check if teacher is already assigned to another section
                var existingAssignment = await _unitOfWork.SectionsRepository
                    .GetSectionByClassTeacherIdAsync(request.TeacherId, cancellationToken);

                if (existingAssignment != null && existingAssignment.Id != request.SectionId)
                {
                    return Result<bool>.Failure(
                        "TeacherAlreadyAssigned",
                        $"Teacher is already assigned as class teacher to section '{existingAssignment.Name}'. " +
                        $"A teacher can be class teacher to only one section at a time."
                    );
                }

                // Domain method handles the business rule: one teacher per section
                section.AssignClassTeacher(request.TeacherId, currentUserId);

                // Persist changes
                await _unitOfWork.SectionsRepository.UpdateAsync(section, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Class teacher {TeacherId} assigned to section {SectionId} by {UserId}",
                    request.TeacherId,
                    request.SectionId,
                    currentUserId
                );

                return Result<bool>.Success(
                    true,
                    $"Class teacher assigned successfully to section '{section.Name}'"
                );
            }
            catch (ClassTeacherAlreadyAssignedException ex)
            {
                _logger.LogWarning(ex,
                    "Attempted to assign class teacher to section {SectionId} that already has one",
                    request.SectionId
                );

                return Result<bool>.Failure(
                    "ClassTeacherAlreadyAssigned",
                    ex.Message
                );
            }
            catch (SectionException ex)
            {
                _logger.LogWarning(ex,
                    "Section validation failed for section {SectionId}",
                    request.SectionId
                );

                return Result<bool>.Failure(
                    "SectionValidationFailed",
                    ex.Message
                );
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while assigning class teacher to section {SectionId}",
                    request.SectionId
                );

                return Result<bool>.Failure(
                    "DomainValidationFailed",
                    ex.Message
                );
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Concurrency conflict while assigning class teacher to section {SectionId}",
                    request.SectionId
                );

                return Result<bool>.Failure(
                    "ConcurrencyConflict",
                    "The section was modified by another user. Please refresh and try again."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while assigning class teacher to section {SectionId}",
                    request.SectionId
                );

                return Result<bool>.Failure(
                    "UnexpectedError",
                    "An unexpected error occurred while assigning the class teacher. Please try again."
                );
            }
        }
    }
}
