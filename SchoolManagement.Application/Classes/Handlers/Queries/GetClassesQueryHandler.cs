using MediatR;
using SchoolManagement.Application.Classes.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SchoolManagement.Application.Models.Result;

namespace SchoolManagement.Application.Classes.Handlers.Queries
{
    public class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, Result<PagedResult<ClassDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClassesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<ClassDto>>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (items, totalCount) = await _unitOfWork.ClassesRepository.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    request.IsActive
                );

                var dtos = items.Select(c => new ClassDto
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    ClassCode = c.Code,
                    Grade = c.Grade,
                    Description = c.Description,
                    TotalSections = c.Sections?.Count ?? 0,
                    TotalStudents = c.Sections?.Sum(s => s.CurrentStrength) ?? 0,
                    IsActive = c.IsActive
                }).ToList();

                var response = new PagedResult<ClassDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<PagedResult<ClassDto>>.Success(response, "Classes fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<PagedResult<ClassDto>>.Failure("Failed to fetch classes.", ex.Message);
            }
        }
    }
}