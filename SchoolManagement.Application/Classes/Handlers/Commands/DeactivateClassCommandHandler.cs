using MediatR;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class DeactivateClassCommandHandler
        : IRequestHandler<DeactivateClassCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateClassCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeactivateClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassesRepository.GetByIdAsync(request.Id, cancellationToken);

                if (classEntity == null)
                    return Result.Failure("Class not found.", $"No class exists with Id: {request.Id}");

                // Deactivate the class
                classEntity.Deactivate();

                await _unitOfWork.ClassesRepository.UpdateAsync(classEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Class deactivated successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to deactivate class.", ex.Message);
            }
        }
    }
}
