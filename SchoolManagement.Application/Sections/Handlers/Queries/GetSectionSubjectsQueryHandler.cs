using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.SectionSubjects.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Queries
{
    public class GetSectionSubjectsQueryHandler : IRequestHandler<GetSectionSubjectsQuery, Result<List<SectionSubjectDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSectionSubjectsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<SectionSubjectDto>>> Handle(GetSectionSubjectsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var subjects = await _unitOfWork.SectionSubjectsRepository.GetBySectionIdAsync(request.SectionId, cancellationToken);

                if (subjects == null || !subjects.Any())
                {
                    return Result<List<SectionSubjectDto>>.Success(new List<SectionSubjectDto>(), "No subjects found for this section.");
                }

                var dtos = subjects.Select(s => new SectionSubjectDto
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

                return Result<List<SectionSubjectDto>>.Success(dtos, "Section subjects fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<List<SectionSubjectDto>>.Failure("Failed to fetch section subjects.", ex.Message);
            }
        }
    }
}
