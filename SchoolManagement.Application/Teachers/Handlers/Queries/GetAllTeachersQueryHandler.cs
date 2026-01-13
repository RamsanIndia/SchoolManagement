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
    public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, Result<PagedResult<TeacherDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllTeachersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<TeacherDto>>> Handle(
            GetAllTeachersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Build query using GetQueryable
                var query = _unitOfWork.TeachersRepository.GetQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(t =>
                        t.Name.FirstName.ToLower().Contains(searchTerm) ||
                        t.Name.LastName.ToLower().Contains(searchTerm) ||
                        t.EmployeeCode.ToLower().Contains(searchTerm) ||
                        t.Email.Value.ToLower().Contains(searchTerm) ||
                        t.PhoneNumber.Value.Contains(searchTerm));
                }

                // Apply active filter
                if (request.IsActive.HasValue)
                {
                    query = query.Where(t => t.IsActive == request.IsActive.Value);
                }

                // Apply department filter
                if (request.DepartmentId.HasValue)
                {
                    query = query.Where(t => t.DepartmentId == request.DepartmentId.Value);
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
                    _ => query.OrderBy(t => t.Name.LastName) // Default: sort by last name
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
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
                    $"Retrieved {dtos.Count} teachers out of {totalCount}"
                );
            }
            catch (Exception ex)
            {
                return Result<PagedResult<TeacherDto>>.Failure(
                    "Failed to fetch teachers.",
                    ex.Message
                );
            }
        }
    }
}