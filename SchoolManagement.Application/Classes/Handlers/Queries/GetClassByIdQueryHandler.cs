using MediatR;
using SchoolManagement.Application.Classes.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Queries
{
    public class GetClassByIdQueryHandler
        : IRequestHandler<GetClassByIdQuery, Result<ClassDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClassByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ClassDto>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassesRepository.GetByIdWithSectionsAsync(request.Id, cancellationToken);

                if (classEntity == null)
                    return Result<ClassDto>.Failure("Class not found.", $"No class exists with Id: {request.Id}");

                var classDto = new ClassDto
                {
                    Id = classEntity.Id,
                    ClassName = classEntity.Name,
                    ClassCode = classEntity.Code,
                    Grade = classEntity.Grade,
                    Description = classEntity.Description,
                    AcademicYearId = classEntity.AcademicYearId,
                    IsActive = classEntity.IsActive,
                    TotalSections = classEntity.Sections?.Count ?? 0,
                    TotalStudents = classEntity.Sections?.Sum(s => s.Capacity.CurrentStrength) ?? 0
                };

                return Result<ClassDto>.Success(classDto);
            }
            catch (Exception ex)
            {
                return Result<ClassDto>.Failure("Failed to retrieve class details.", ex.Message);
            }
        }
    }
}
