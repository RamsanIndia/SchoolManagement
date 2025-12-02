using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Sections.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Queries
{
    public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, Result<List<SectionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSectionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<SectionDto>>> Handle(GetSectionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch data
                var sections = request.ClassId.HasValue
                    ? await _unitOfWork.SectionsRepository.GetByClassIdAsync(request.ClassId.Value, cancellationToken)
                    : await _unitOfWork.SectionsRepository.GetActiveSectionsAsync(cancellationToken);

                if (sections == null || !sections.Any())
                {
                    // Return success with empty list (caller can decide)
                    return Result<List<SectionDto>>.Success(new List<SectionDto>(), "No sections found.");
                }

                // Map to DTO
                var dtos = sections.Select(s => new SectionDto
                {
                    Id = s.Id,
                    ClassId = s.ClassId,
                    ClassName = s.Class?.Name,
                    SectionName = s.Name,
                    Capacity = s.Capacity,
                    CurrentStrength = s.CurrentStrength,
                    AvailableSeats = s.Capacity - s.CurrentStrength,
                    RoomNumber = s.RoomNumber,
                    ClassTeacherId = s.ClassTeacherId,
                    // If you have navigation property populated, use it; otherwise resolve via teacher repo/service
                    //ClassTeacherName = s.ClassTeacher?.FullName,
                    IsActive = s.IsActive,
                    TotalSubjects = s.SectionSubjects?.Count ?? 0
                }).ToList();

                return Result<List<SectionDto>>.Success(dtos, "Sections fetched successfully.");
            }
            catch (System.Exception ex)
            {
                // Log the exception as needed, then return structured failure
                return Result<List<SectionDto>>.Failure("Failed to fetch sections.", ex.Message);
            }
        }
    }
}
