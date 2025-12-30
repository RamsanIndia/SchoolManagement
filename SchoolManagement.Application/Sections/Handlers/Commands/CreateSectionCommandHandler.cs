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

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateSectionCommandHandler> _logger;

        public CreateSectionCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreateSectionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Guid>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating section {SectionName} for class {ClassId}",
                    request.SectionName,
                    request.ClassId);

                // Validate class exists
                var classExists = await _unitOfWork.ClassesRepository
                    .ExistsAsync(request.ClassId, cancellationToken);

                if (!classExists)
                {
                    _logger.LogWarning("Class {ClassId} not found", request.ClassId);
                    return Result<Guid>.Failure(
                        "Class not found.",
                        $"Class with ID '{request.ClassId}' does not exist.");
                }

                // Validate section name uniqueness within the class
                var sectionExists = await _unitOfWork.SectionsRepository
                    .IsSectionNameExistsAsync(request.ClassId, request.SectionName);

                if (sectionExists)
                {
                    _logger.LogWarning(
                        "Section name {SectionName} already exists for class {ClassId}",
                        request.SectionName,
                        request.ClassId);

                    return Result<Guid>.Failure(
                        "Section name already exists.",
                        $"A section named '{request.SectionName}' already exists for this class.");
                }

                // Create section using factory method
                Section section;
                try
                {
                    section = Section.Create(
                        request.ClassId,
                        request.SectionName,
                        request.Capacity,
                        request.RoomNumber);
                }
                catch (SectionException ex)
                {
                    _logger.LogWarning(ex, "Domain validation failed while creating section");
                    return Result<Guid>.Failure("Validation failed.", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Invalid argument while creating section");
                    return Result<Guid>.Failure("Invalid input.", ex.Message);
                }

                try
                {
                    // Add section to repository
                    await _unitOfWork.SectionsRepository.AddAsync(section, cancellationToken);

                    // Save changes
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Successfully created section {SectionId} with name {SectionName}",
                        section.Id,
                        section.Name);

                    return Result<Guid>.Success(
                        section.Id,
                        "Section created successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while saving section");
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating section");
                return Result<Guid>.Failure(
                    "An error occurred while creating the section.",
                    ex.Message);
            }
        }
    }
}
