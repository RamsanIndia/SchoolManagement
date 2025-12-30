using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Sections.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Features.SectionSubjects.Handlers.Commands
{
    public class MapSubjectCommandHandler : IRequestHandler<MapSubjectCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MapSubjectCommandHandler> _logger;

        public MapSubjectCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MapSubjectCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            MapSubjectCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get current user
                var currentUser = _currentUserService.Username ?? "System";

                // Validate section exists and is active
                var section = await _unitOfWork.SectionsRepository
                    .GetByIdAsync(request.SectionId, cancellationToken);

                if (section == null)
                {
                    return Result<Guid>.Failure(
                        "SectionNotFound",
                        $"Section with ID '{request.SectionId}' not found"
                    );
                }

                if (!section.IsActive)
                {
                    return Result<Guid>.Failure(
                        "SectionInactive",
                        $"Cannot map subject to inactive section '{section.Name}'"
                    );
                }

                // Check if subject is already mapped
                var exists = await _unitOfWork.SectionSubjectsRepository.IsSubjectMappedAsync(
                    request.SectionId,
                    request.SubjectId,
                    cancellationToken
                );

                if (exists)
                {
                    return Result<Guid>.Failure(
                        "SubjectAlreadyMapped",
                        $"The subject '{request.SubjectName}' is already mapped to section '{section.Name}'"
                    );
                }

                // Validate teacher exists and is active
                var teacher = await _unitOfWork.TeachersRepository
                    .GetByIdAsync(request.TeacherId, cancellationToken);

                if (teacher == null)
                {
                    return Result<Guid>.Failure(
                        "TeacherNotFound",
                        $"Teacher with ID '{request.TeacherId}' not found"
                    );
                }

                if (!teacher.IsActive)
                {
                    return Result<Guid>.Failure(
                        "TeacherInactive",
                        $"Cannot assign inactive teacher '{request.TeacherName}' to teach this subject"
                    );
                }

                // Check teacher workload (warning, not blocking)
                if (!teacher.CanAcceptMoreAssignments())
                {
                    _logger.LogWarning(
                        "Teacher {TeacherId} is nearing maximum workload ({TotalPeriods} periods) but assignment is proceeding",
                        teacher.Id,
                        teacher.GetTotalWeeklyPeriods()
                    );
                }

                var sectionSubject = SectionSubject.Create(
                    request.SectionId,
                    request.SubjectId,
                    request.SubjectName,
                    request.SubjectCode,
                    request.TeacherId,
                    request.TeacherName,
                    request.WeeklyPeriods,
                    request.IsMandatory
                );

                await _unitOfWork.SectionSubjectsRepository.AddAsync(
                    sectionSubject,
                    cancellationToken
                );

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Subject {SubjectName} (ID: {SubjectId}) mapped to section {SectionName} (ID: {SectionId}) with teacher {TeacherName} by {User}",
                    request.SubjectName,
                    request.SubjectId,
                    section.Name,
                    request.SectionId,
                    request.TeacherName,
                    currentUser
                );

                return Result<Guid>.Success(
                    sectionSubject.Id,
                    $"Subject '{request.SubjectName}' mapped to section '{section.Name}' successfully"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex,
                    "Validation failed while mapping subject {SubjectId} to section {SectionId}",
                    request.SubjectId,
                    request.SectionId
                );

                return Result<Guid>.Failure(
                    "ValidationFailed",
                    ex.Message
                );
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain validation failed while mapping subject {SubjectId} to section {SectionId}",
                    request.SubjectId,
                    request.SectionId
                );

                return Result<Guid>.Failure(
                    "DomainValidationFailed",
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while mapping subject {SubjectId} to section {SectionId}",
                    request.SubjectId,
                    request.SectionId
                );

                return Result<Guid>.Failure(
                    "UnexpectedError",
                    "An unexpected error occurred while mapping the subject. Please try again."
                );
            }
        }
    }
}
