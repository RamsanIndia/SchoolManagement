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
    public class GetTeachersByDepartmentQueryHandler
        : IRequestHandler<GetTeachersByDepartmentQuery, Result<PagedResult<TeacherDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTeachersByDepartmentQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<TeacherDto>>> Handle(
            GetTeachersByDepartmentQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Verify department exists
                var department = await _unitOfWork.DepartmentRepository
                    .GetByIdAsync(request.DepartmentId, cancellationToken);

                if (department == null)
                {
                    return Result<PagedResult<TeacherDto>>.Failure(
                        "DepartmentNotFound",
                        $"Department with ID '{request.DepartmentId}' not found"
                    );
                }

                // Build query
                var query = _unitOfWork.TeachersRepository
                    .GetQueryable()
                    .Where(t => t.DepartmentId == request.DepartmentId);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(t =>
                        t.Name.FirstName.ToLower().Contains(searchTerm) ||
                        t.Name.LastName.ToLower().Contains(searchTerm) ||
                        t.EmployeeCode.ToLower().Contains(searchTerm));
                }

                // Apply active filter
                if (request.IsActive.HasValue)
                {
                    query = query.Where(t => t.IsActive == request.IsActive.Value);
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "firstname" => request.SortDirection == "desc"
                        ? query.OrderByDescending(t => t.Name.FirstName)
                        : query.OrderBy(t => t.Name.FirstName),
                    "employeeid" => request.SortDirection == "desc"
                        ? query.OrderByDescending(t => t.EmployeeCode)
                        : query.OrderBy(t => t.EmployeeCode),
                    "dateofjoining" => request.SortDirection == "desc"
                        ? query.OrderByDescending(t => t.DateOfJoining)
                        : query.OrderBy(t => t.DateOfJoining),
                    "experience" => request.SortDirection == "desc"
                        ? query.OrderByDescending(t => t.TotalYearsOfExperience)
                        : query.OrderBy(t => t.TotalYearsOfExperience),
                    _ => query.OrderBy(t => t.Name.LastName)
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and include department
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Include(t => t.Department)
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var dtos = items.Select(t => new TeacherDto
                {
                    Id = t.Id,
                    FirstName = t.Name.FirstName,
                    LastName = t.Name.LastName,
                    FullName = t.FullName,
                    Email = t.Email.Value,
                    PhoneNumber = t.PhoneNumber.Value,
                    EmployeeId = t.EmployeeCode,
                    DateOfJoining = t.DateOfJoining,
                    Qualification = t.Qualification,
                    Experience = t.PriorExperience,
                    TotalExperience = t.TotalYearsOfExperience,
                    DepartmentId = t.DepartmentId,
                    DepartmentName = t.Department?.Name,
                    IsActive = t.IsActive,
                    TotalTeachingAssignments = t.GetTotalTeachingAssignments(),
                    TotalWeeklyPeriods = t.GetTotalWeeklyPeriods()
                }).ToList();

                // Create paged result
                var response = new PagedResult<TeacherDto>(
                    dtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result<PagedResult<TeacherDto>>.Success(
                    response,
                    $"Retrieved {dtos.Count} teachers from department '{department.Name}'"
                );
            }
            catch (Exception ex)
            {
                return Result<PagedResult<TeacherDto>>.Failure(
                    "Failed to fetch teachers by department.",
                    ex.Message
                );
            }
        }
    }
}
