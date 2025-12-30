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
    public class GetTimeTableQueryHandler
        : IRequestHandler<GetTimeTableQuery, Result<TimeTableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTimeTableQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TimeTableDto>> Handle(GetTimeTableQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if section exists
                var section = await _unitOfWork.SectionsRepository.GetByIdWithDetailsAsync(request.SectionId,cancellationToken);
                if (section == null)
                    return Result<TimeTableDto>.Failure("Section not found.");

                // Get timetable entries
                var entries = await _unitOfWork.TimeTablesRepository.GetBySectionIdAsync(request.SectionId,cancellationToken);

                var schedule = new Dictionary<DayOfWeek, List<TimeTableEntryDto>>();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                        continue;

                    var dayEntries = entries
                        .Where(e => e.DayOfWeek == day)
                        .OrderBy(e => e.PeriodNumber)
                        .Select(e => new TimeTableEntryDto
                        {
                            Id = e.Id,
                            PeriodNumber = e.PeriodNumber,
                            StartTime = e.TimePeriod.StartTime,
                            EndTime = e.TimePeriod.StartTime,
                            SubjectId = e.SubjectId,
                            RoomNumber = e.RoomNumber
                        })
                        .ToList();

                    schedule[day] = dayEntries;
                }

                var dto = new TimeTableDto
                {
                    SectionId = section.Id,
                    SectionName = section.Name,
                    ClassName = section.Class?.Name,
                    Schedule = schedule
                };

                return Result<TimeTableDto>.Success(dto, "Timetable fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<TimeTableDto>.Failure("Failed to fetch timetable.", ex.Message);
            }
        }
    }
}
