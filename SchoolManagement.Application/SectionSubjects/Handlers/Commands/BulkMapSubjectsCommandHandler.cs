using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.SectionSubjects.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Features.SectionSubjects.Handlers.Commands
{
    public class BulkMapSubjectsCommandHandler
        : IRequestHandler<BulkMapSubjectsCommand, Result<BulkMapResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<BulkMapSubjectsCommandHandler> _logger;

        public BulkMapSubjectsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<BulkMapSubjectsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<BulkMapResult>> Handle(
            BulkMapSubjectsCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BulkMapResult
            {
                TotalProcessed = request.SubjectMappings.Count
            };

            try
            {
                // Get current user
                var currentUser = _currentUserService.Username ?? "System";

                // Validate section exists
                var section = await _unitOfWork.SectionsRepository
                    .GetByIdAsync(request.SectionId, cancellationToken);

                if (section == null)
                {
                    return Result<BulkMapResult>.Failure(
                        "SectionNotFound",
                        $"Section with ID '{request.SectionId}' not found"
                    );
                }

                // Validate section is active
                if (!section.IsActive)
                {
                    return Result<BulkMapResult>.Failure(
                        "SectionInactive",
                        $"Cannot map subjects to inactive section '{section.Name}'"
                    );
                }

                // Get existing mappings for this section
                var existingMappings = await _unitOfWork.SectionSubjectsRepository
                    .GetBySectionIdAsync(request.SectionId, cancellationToken);

                var existingSubjectIds = existingMappings
                    .Select(ss => ss.SubjectId)
                    .ToHashSet();

                // Process each mapping
                foreach (var mapping in request.SubjectMappings)
                {
                    try
                    {
                        // Check if subject is already mapped
                        if (existingSubjectIds.Contains(mapping.SubjectId))
                        {
                            result.FailureCount++;
                            result.Errors.Add(new BulkMapError
                            {
                                SubjectId = mapping.SubjectId,
                                SubjectName = mapping.SubjectName,
                                ErrorMessage = $"Subject '{mapping.SubjectName}' is already mapped to this section"
                            });
                            continue;
                        }

                        // Validate teacher exists and is active
                        var teacher = await _unitOfWork.TeachersRepository
                            .GetByIdAsync(mapping.TeacherId, cancellationToken);

                        if (teacher == null)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new BulkMapError
                            {
                                SubjectId = mapping.SubjectId,
                                SubjectName = mapping.SubjectName,
                                ErrorMessage = $"Teacher with ID '{mapping.TeacherId}' not found"
                            });
                            continue;
                        }

                        if (!teacher.IsActive)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new BulkMapError
                            {
                                SubjectId = mapping.SubjectId,
                                SubjectName = mapping.SubjectName,
                                ErrorMessage = $"Teacher '{mapping.TeacherName}' is inactive"
                            });
                            continue;
                        }

                        // Check teacher workload
                        if (!teacher.CanAcceptMoreAssignments())
                        {
                            result.WarningCount++;
                            result.Warnings.Add(new BulkMapWarning
                            {
                                SubjectId = mapping.SubjectId,
                                SubjectName = mapping.SubjectName,
                                WarningMessage = $"Teacher '{teacher.FullName}' is nearing maximum workload ({teacher.GetTotalWeeklyPeriods()} periods)"
                            });
                        }

                        // Create section subject using factory method
                        var sectionSubject = SectionSubject.Create(
                            request.SectionId,
                            mapping.SubjectId,
                            mapping.SubjectName,
                            mapping.SubjectCode,
                            mapping.TeacherId,
                            mapping.TeacherName,
                            mapping.WeeklyPeriods,
                            mapping.IsMandatory
                            
                        );

                        await _unitOfWork.SectionSubjectsRepository.AddAsync(
                            sectionSubject,
                            cancellationToken
                        );

                        result.SuccessCount++;
                        result.SuccessfulMappings.Add(new SuccessfulMapping
                        {
                            SubjectId = mapping.SubjectId,
                            SubjectName = mapping.SubjectName,
                            SubjectCode = mapping.SubjectCode,
                            TeacherName = mapping.TeacherName,
                            WeeklyPeriods = mapping.WeeklyPeriods
                        });

                        _logger.LogInformation(
                            "Subject {SubjectName} mapped to section {SectionId} by {User}",
                            mapping.SubjectName,
                            request.SectionId,
                            currentUser
                        );
                    }
                    catch (DomainException ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BulkMapError
                        {
                            SubjectId = mapping.SubjectId,
                            SubjectName = mapping.SubjectName,
                            ErrorMessage = ex.Message
                        });

                        _logger.LogWarning(ex,
                            "Domain validation failed for subject {SubjectId}",
                            mapping.SubjectId
                        );
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BulkMapError
                        {
                            SubjectId = mapping.SubjectId,
                            SubjectName = mapping.SubjectName,
                            ErrorMessage = $"Unexpected error: {ex.Message}"
                        });

                        _logger.LogError(ex,
                            "Failed to map subject {SubjectId} to section {SectionId}",
                            mapping.SubjectId,
                            request.SectionId
                        );
                    }
                }

                // Save all successful mappings
                if (result.SuccessCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Determine overall result message
                var message = result.SuccessCount == result.TotalProcessed
                    ? $"All {result.SuccessCount} subjects mapped successfully"
                    : result.SuccessCount > 0
                        ? $"Bulk mapping completed: {result.SuccessCount} succeeded, {result.FailureCount} failed"
                        : "Bulk mapping failed: No subjects were mapped";

                _logger.LogInformation(
                    "Bulk mapping completed for section {SectionId}. Success: {Success}, Failed: {Failed}, Warnings: {Warnings}",
                    request.SectionId,
                    result.SuccessCount,
                    result.FailureCount,
                    result.WarningCount
                );

                return Result<BulkMapResult>.Success(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error during bulk mapping for section {SectionId}",
                    request.SectionId
                );

                return Result<BulkMapResult>.Failure(
                    "BulkMappingFailed",
                    "An unexpected error occurred during bulk mapping. Please try again."
                );
            }
        }
    }
}
