using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Teachers.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Handlers.Queries
{
    public class GetTeacherWorkloadQueryHandler
        : IRequestHandler<GetTeacherWorkloadQuery, Result<TeacherWorkloadDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTeacherWorkloadQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TeacherWorkloadDto>> Handle(
            GetTeacherWorkloadQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Build query with eager loading using GetQueryable
                var teacher = await _unitOfWork.TeachersRepository
                    .GetQueryable()
                    .Include(t => t.TeachingAssignments)
                    .Include(t => t.ClassTeacherSections)
                        .ThenInclude(s => s.Class)
                    .Include(t => t.Department)
                    .FirstOrDefaultAsync(t => t.Id == request.TeacherId, cancellationToken);

                if (teacher == null)
                {
                    return Result<TeacherWorkloadDto>.Failure(
                        "TeacherNotFound",
                        $"Teacher with ID '{request.TeacherId}' not found"
                    );
                }

                // Map to workload DTO
                var workloadDto = new TeacherWorkloadDto
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.FullName,
                    EmployeeId = teacher.EmployeeCode,
                    DepartmentName = teacher.Department?.Name,
                    Email = teacher.Email.Value,
                    PhoneNumber = teacher.PhoneNumber.Value,
                    TotalWeeklyPeriods = teacher.GetTotalWeeklyPeriods(),
                    TotalAssignments = teacher.GetTotalTeachingAssignments(),
                    CanAcceptMore = teacher.CanAcceptMoreAssignments(),
                    //MaxWeeklyPeriods = teacher.MaxWeeklyPeriods, // Add this property if available
                    IsActive = teacher.IsActive,

                    // Teaching assignments
                    Assignments = teacher.TeachingAssignments
                        .Select(ta => new AssignmentDto
                        {
                            SectionId = ta.SectionId,
                            SubjectId = ta.SubjectId,
                            SubjectName = ta.SubjectName,
                            SubjectCode = ta.SubjectCode,
                            WeeklyPeriods = ta.WeeklyPeriods,
                            IsMandatory = ta.IsMandatory
                        })
                        .OrderBy(a => a.SubjectName)
                        .ToList(),

                    // Class teacher sections (only active)
                    ClassTeacherSections = teacher.ClassTeacherSections
                        .Where(s => s.IsActive)
                        .Select(s => new ClassTeacherSectionDto
                        {
                            SectionId = s.Id,
                            SectionName = s.Name,
                            ClassName = s.Class?.Name,
                            CurrentStrength = s.Capacity?.CurrentStrength ?? 0,
                            MaxCapacity = s.Capacity?.MaxCapacity ?? 0
                        })
                        .OrderBy(s => s.ClassName)
                        .ThenBy(s => s.SectionName)
                        .ToList()
                };

                return Result<TeacherWorkloadDto>.Success(
                    workloadDto,
                    "Teacher workload retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return Result<TeacherWorkloadDto>.Failure(
                    "Failed to fetch teacher workload.",
                    ex.Message
                );
            }
        }
    }
}
