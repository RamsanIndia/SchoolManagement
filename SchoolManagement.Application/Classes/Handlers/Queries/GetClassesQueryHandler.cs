using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, Result<PagedResult<ClassDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClassesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<ClassDto>>> Handle(
            GetClassesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Build query using GetQueryable
                var query = _unitOfWork.ClassesRepository.GetQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.Name.ToLower().Contains(searchTerm) ||
                        c.Code.ToLower().Contains(searchTerm) ||
                        (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
                }

                // Apply active filter
                if (request.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == request.IsActive.Value);
                }

                // Apply academic year filter
                if (request.AcademicYearId.HasValue)
                {
                    query = query.Where(c => c.AcademicYearId == request.AcademicYearId.Value);
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "code" => request.SortDirection == "desc"
                        ? query.OrderByDescending(c => c.Code)
                        : query.OrderBy(c => c.Code),
                    "grade" => request.SortDirection == "desc"
                        ? query.OrderByDescending(c => c.Grade)
                        : query.OrderBy(c => c.Grade),
                    "createdat" => request.SortDirection == "desc"
                        ? query.OrderByDescending(c => c.CreatedAt)
                        : query.OrderBy(c => c.CreatedAt),
                    _ => query.OrderBy(c => c.Name) // Default: sort by name
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and include related data
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Include(c => c.Sections) // Include sections for aggregation
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var dtos = items.Select(c => new ClassDto
                {
                    Id = c.Id,
                    AcademicYearId = c.AcademicYearId,
                    ClassName = c.Name,
                    ClassCode = c.Code,
                    Grade = c.Grade,
                    Description = c.Description,
                    TotalSections = c.Sections?.Count ?? 0,
                    TotalStudents = c.Sections?.Sum(s => s.Capacity.CurrentStrength) ?? 0,
                    IsActive = c.IsActive
                }).ToList();

                // Create paged result
                var response = new PagedResult<ClassDto>(
                    dtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result<PagedResult<ClassDto>>.Success(
                    response,
                    "Classes fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<PagedResult<ClassDto>>.Failure(
                    "Failed to fetch classes.",
                    ex.Message);
            }
        }
    }
}
