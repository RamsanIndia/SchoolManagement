using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.SectionSubjects.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Handlers.Commands
{
    public class RemoveSubjectMappingCommandHandler
        : IRequestHandler<RemoveSubjectMappingCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveSubjectMappingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RemoveSubjectMappingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mapping = await _unitOfWork.SectionSubjectsRepository.GetByIdAsync(request.MappingId, cancellationToken);
                if (mapping == null)
                    return Result.Failure("Subject mapping not found");

                await _unitOfWork.SectionSubjectsRepository.DeleteAsync(mapping, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Subject mapping removed successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to remove subject mapping", ex.Message);
            }
        }
    }
}
