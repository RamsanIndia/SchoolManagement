using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Sections.Commands;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Commands
{
    public class CreateSectionCommandHandler
        : IRequestHandler<CreateSectionCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateSectionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            // Validate if section already exists
            var exists = await _unitOfWork.SectionsRepository
                .IsSectionNameExistsAsync(request.ClassId, request.SectionName);

            if (exists)
            {
                return Result<Guid>.Failure(
                    $"Section name already exists for this class.",
                    $"'{request.SectionName}' is already used."
                );
            }

            // Create new Section entity
            var section = new Section(
                request.ClassId,
                request.SectionName,
                request.Capacity,
                request.RoomNumber
            );

            await _unitOfWork.SectionsRepository.AddAsync(section, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(section.Id, "Section created successfully.");
        }
    }
}
