using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Queries
{
    public class GetTeacherTimeTableQueryHandler
        : IRequestHandler<GetTeacherTimeTableQuery, Result<TeacherTimeTableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTeacherTimeTableQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TeacherTimeTableDto>> Handle(GetTeacherTimeTableQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var entries = await _unitOfWork.TimeTablesRepository.GetByTeacherIdAsync(request.TeacherId);

                if (entries == null || !entries.Any())
                    return Result<TeacherTimeTableDto>.Failure("No timetable found for the specified teacher.");

                var schedule = new Dictionary<DayOfWeek, List<TeacherTimeTableEntryDto>>();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                        continue;

                    var dayEntries = entries
                        .Where(e => e.DayOfWeek == day)
                        .OrderBy(e => e.PeriodNumber)
                        .Select(e => new TeacherTimeTableEntryDto
                        {
                            Id = e.Id,
                            PeriodNumber = e.PeriodNumber,
                            StartTime = e.StartTime,
                            EndTime = e.EndTime,
                            ClassName = e.Section?.Class?.Name,
                            SectionName = e.Section?.Name,
                            RoomNumber = e.RoomNumber
                        })
                        .ToList();

                    schedule[day] = dayEntries;
                }

                var dto = new TeacherTimeTableDto
                {
                    TeacherId = request.TeacherId,
                    //TeacherName = entries.First().TeacherName,
                    Schedule = schedule
                };

                return Result<TeacherTimeTableDto>.Success(dto, "Teacher timetable fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<TeacherTimeTableDto>.Failure("Failed to retrieve teacher timetable.", ex.Message);
            }
        }
    }
}