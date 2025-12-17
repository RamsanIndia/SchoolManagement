using MediatR;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class UpdateClassCommandHandler
        : IRequestHandler<UpdateClassCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateClassCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassesRepository.GetByIdAsync(request.Id, cancellationToken);

                if (classEntity == null)
                    return Result.Failure("Class not found.", $"No class exists with Id: {request.Id}");

                // Check if the updated class code is already used by another class
                var exists = await _unitOfWork.ClassesRepository.IsClassCodeExistsAsync(request.ClassCode, cancellationToken, request.Id);
                if (exists)
                    return Result.Failure($"Class code '{request.ClassCode}' already exists.");

                // Update class details
                classEntity.UpdateDetails(
                    request.ClassName,
                    request.ClassCode,
                    request.Grade,
                    request.Description
                );

                await _unitOfWork.ClassesRepository.UpdateAsync(classEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Class updated successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to update class.", ex.Message);
            }
        }
    }
}
