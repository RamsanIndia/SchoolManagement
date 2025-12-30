using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.SectionSubjects.Queries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Handlers.Queries
{
    public class GetSectionSubjectsQueryHandler : IRequestHandler<GetSectionSubjectsQuery, Result<PagedResult<SectionSubjectDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSectionSubjectsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<SectionSubjectDto>>> Handle(
            GetSectionSubjectsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Build query fluently
                var query = _unitOfWork.SectionSubjectsRepository
                    .GetQueryable()
                    .Where(s => s.SectionId == request.SectionId);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(s =>
                        s.SubjectName.Contains(request.SearchTerm) ||
                        s.SubjectCode.Contains(request.SearchTerm) ||
                        s.TeacherName.Contains(request.SearchTerm));
                }

                // Apply mandatory filter
                if (request.IsMandatory.HasValue)
                {
                    query = query.Where(s => s.IsMandatory == request.IsMandatory.Value);
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "teachername" => request.SortDirection == "desc"
                        ? query.OrderByDescending(s => s.TeacherName)
                        : query.OrderBy(s => s.TeacherName),
                    "weeklyperiods" => request.SortDirection == "desc"
                        ? query.OrderByDescending(s => s.WeeklyPeriods)
                        : query.OrderBy(s => s.WeeklyPeriods),
                    _ => query.OrderBy(s => s.SubjectName)
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var dtos = items.Select(s => new SectionSubjectDto
                {
                    Id = s.Id,
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    SubjectCode = s.SubjectCode,
                    TeacherId = s.TeacherId,
                    TeacherName = s.TeacherName,
                    WeeklyPeriods = s.WeeklyPeriods,
                    IsMandatory = s.IsMandatory
                }).ToList();

                // Create paginated result
                var response = new PagedResult<SectionSubjectDto>(
                    dtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result<PagedResult<SectionSubjectDto>>.Success(
                    response,
                    "Section subjects fetched successfully."
                );
            }
            catch (Exception ex)
            {
                return Result<PagedResult<SectionSubjectDto>>.Failure(
                    "Failed to fetch section subjects.",
                    ex.Message
                );
            }
        }
    }
}
