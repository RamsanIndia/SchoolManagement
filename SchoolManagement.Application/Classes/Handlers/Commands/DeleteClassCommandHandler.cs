using MediatR;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class DeleteClassCommandHandler
        : IRequestHandler<DeleteClassCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClassCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassesRepository.GetByIdAsync(request.Id, cancellationToken);

                if (classEntity == null)
                    return Result.Failure("Class not found.", $"No class exists with Id: {request.Id}");

                await _unitOfWork.ClassesRepository.DeleteAsync(classEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Class deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to delete class.", ex.Message);
            }
        }
    }
}
