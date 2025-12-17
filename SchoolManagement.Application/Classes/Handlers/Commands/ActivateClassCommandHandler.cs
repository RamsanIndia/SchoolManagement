using MediatR;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class ActivateClassCommandHandler
        : IRequestHandler<ActivateClassCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivateClassCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ActivateClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassesRepository.GetByIdAsync(request.Id, cancellationToken);

                if (classEntity == null)
                    return Result.Failure("Class not found.", $"No class exists with Id: {request.Id}");

                // Activate the class
                classEntity.Activate();

                await _unitOfWork.ClassesRepository.UpdateAsync(classEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Class activated successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to activate class.", ex.Message);
            }
        }
    }
}
