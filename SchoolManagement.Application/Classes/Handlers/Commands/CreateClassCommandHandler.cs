using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{


    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Result<ClassDto>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateClassCommandHandler> _logger;
        private readonly ICorrelationIdService _correlationIdService;

        public CreateClassCommandHandler(
            IClassRepository classRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateClassCommandHandler> logger,
            ICorrelationIdService correlationIdService)
        {
            _classRepository = classRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _correlationIdService = correlationIdService;
        }

        public async Task<Result<ClassDto>> Handle(
            CreateClassCommand request,
            CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdService.GetCorrelationId();

            try
            {
                _logger.LogInformation(
                    "Creating new class. CorrelationId: {CorrelationId}, ClassName: {ClassName}",
                    correlationId, request.ClassName);

                // ✅ Domain method doesn't need infrastructure data
                var classEntity = Class.Create(
                    request.ClassName,
                    request.ClassCode,
                    request.Grade,
                    request.Description,
                    request.AcademicYearId);

                await _classRepository.AddAsync(classEntity, cancellationToken);

                // ✅ CorrelationId added to outbox in DbContext
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Class created successfully. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, classEntity.Id);
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

                return Result<ClassDto>.Success(classDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating class. CorrelationId: {CorrelationId}",
                    correlationId);
                return Result<ClassDto>.Failure("Failed to create class.");
            }
        }
    }
}