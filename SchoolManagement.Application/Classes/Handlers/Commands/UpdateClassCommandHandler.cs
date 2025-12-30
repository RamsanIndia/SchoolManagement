using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class UpdateClassCommandHandler
        : IRequestHandler<UpdateClassCommand, Result<ClassDto>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClassCommandHandler> _logger;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly ICurrentUserService _currentUserService;

        public UpdateClassCommandHandler(
            IClassRepository classRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateClassCommandHandler> logger,
            IpAddressHelper ipAddressHelper,
            ICorrelationIdService correlationIdService,
            ICurrentUserService currentUserService)
        {
            _classRepository = classRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ipAddressHelper = ipAddressHelper;
            _correlationIdService = correlationIdService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ClassDto>> Handle(
            UpdateClassCommand request,
            CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            var userId = _currentUserService.Username;

            try
            {
                _logger.LogInformation(
                    "Starting class update. CorrelationId: {CorrelationId}, ClassId: {ClassId}, ClassCode: {ClassCode}",
                    correlationId, request.Id, request.ClassCode);

                // Retrieve the class entity
                var classEntity = await _classRepository.GetByIdAsync(
                    request.Id,
                    cancellationToken);

                if (classEntity == null)
                {
                    _logger.LogWarning(
                        "Class not found. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                        correlationId, request.Id);
                    return Result<ClassDto>.Failure(
                        "Class not found.",
                        $"No class exists with Id: {request.Id}");
                }

                // Defense in depth: Check if updated class code conflicts with another class
                var exists = await _classRepository.IsClassCodeExistsAsync(
                    request.ClassCode,
                    cancellationToken,
                    request.Id);

                if (exists)
                {
                    _logger.LogWarning(
                        "Duplicate class code during update. CorrelationId: {CorrelationId}, ClassCode: {ClassCode}",
                        correlationId, request.ClassCode);
                    return Result<ClassDto>.Failure(
                        $"Class code '{request.ClassCode}' already exists.");
                }

                // Update class details using domain method (raises ClassUpdatedEvent)
                classEntity.UpdateDetails(
                    className: request.ClassName,
                    classCode: request.ClassCode,
                    grade: request.Grade,
                    description: request.Description,
                    userId);

                // No need to call UpdateAsync - EF tracks changes automatically
                // await _unitOfWork.ClassesRepository.UpdateAsync(classEntity, cancellationToken);

                // Persist changes and dispatch domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Class updated successfully. CorrelationId: {CorrelationId}, ClassId: {ClassId}, ClassCode: {ClassCode}",
                    correlationId, classEntity.Id, classEntity.Code);

                // Map to DTO (no direct entity exposure)
                var classDto = new ClassDto
                {
                    Id = classEntity.Id,
                    ClassName = classEntity.Name,
                    ClassCode = classEntity.Code,
                    Grade = classEntity.Grade,
                    Description = classEntity.Description,
                    AcademicYearId = classEntity.AcademicYearId,
                    IsActive = classEntity.IsActive,
                    Capacity = classEntity.Capacity,
                    
                };

                return Result<ClassDto>.Success(classDto, "Class updated successfully.");
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex,
                    "Domain validation failed during class update. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, request.Id);
                return Result<ClassDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error during class update. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, request.Id);
                return Result<ClassDto>.Failure("Failed to update class. Please try again later.");
            }
        }
    }
}
