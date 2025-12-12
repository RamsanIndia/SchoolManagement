using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.SectionSubjects.Commands;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Handlers.Commands
{
    public class BulkMapSubjectsCommandHandler
        : IRequestHandler<BulkMapSubjectsCommand, Result<BulkMapResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public BulkMapSubjectsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<BulkMapResult>> Handle(BulkMapSubjectsCommand request, CancellationToken cancellationToken)
        {
            var result = new BulkMapResult();

            try
            {
                foreach (var mapping in request.SubjectMappings)
                {
                    try
                    {
                        var exists = await _unitOfWork.SectionSubjectsRepository.IsSubjectMappedAsync(
                            request.SectionId,
                            mapping.SubjectId,
                            cancellationToken
                        );

                        if (exists)
                        {
                            result.FailureCount++;
                            result.Errors.Add($"Subject {mapping.SubjectName} is already mapped");
                            continue;
                        }

                        var sectionSubject = new SectionSubject(
                            request.SectionId,
                            mapping.SubjectId,
                            mapping.SubjectName,
                            mapping.SubjectCode,
                            mapping.TeacherId,
                            mapping.TeacherName,
                            mapping.WeeklyPeriods,
                            mapping.IsMandatory
                        );

                        await _unitOfWork.SectionSubjectsRepository.AddAsync(sectionSubject, cancellationToken);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Failed to map {mapping.SubjectName}: {ex.Message}");
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<BulkMapResult>.Success(result, "Bulk mapping completed.");
            }
            catch (Exception ex)
            {
                return Result<BulkMapResult>.Failure("Failed to process bulk mapping.", ex.Message);
            }
        }
    }
}
