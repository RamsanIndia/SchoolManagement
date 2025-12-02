using MediatR;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class CreateClassCommandHandler
        : IRequestHandler<CreateClassCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateClassCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if class code already exists
                var exists = await _unitOfWork.ClassesRepository.IsClassCodeExistsAsync(request.ClassCode, cancellationToken);
                if (exists)
                    return Result<Guid>.Failure($"Class code '{request.ClassCode}' already exists.");

                // Create new class entity
                var classEntity = new Class(
                    request.ClassName,
                    request.ClassCode,
                    request.Grade,
                    request.Description,
                    request.AcademicYearId
                );

                await _unitOfWork.ClassesRepository.AddAsync(classEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(classEntity.Id, "Class created successfully.");
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure("Failed to create class.", ex.Message);
            }
        }
    }
}
