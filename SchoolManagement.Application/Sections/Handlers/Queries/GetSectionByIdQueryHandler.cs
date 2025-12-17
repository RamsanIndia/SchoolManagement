using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Sections.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Queries
{
    public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, Result<SectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSectionByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<SectionDto>> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var section = await _unitOfWork.SectionsRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
                if (section == null)
                    return Result<SectionDto>.Failure("Section not found");

                var dto = new SectionDto
                {
                    Id = section.Id,
                    ClassId = section.ClassId,
                    ClassName = section.Class?.Name,
                    SectionName = section.Name,
                    Capacity = section.Capacity,
                    CurrentStrength = section.CurrentStrength,
                    RoomNumber = section.RoomNumber,
                    ClassTeacherId = section.ClassTeacherId,
                    ClassTeacherName = null, // TODO: Fetch from teacher service
                    IsActive = section.IsActive
                };

                return Result<SectionDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<SectionDto>.Failure("Failed to retrieve section details", ex.Message);
            }
        }
    }
}
