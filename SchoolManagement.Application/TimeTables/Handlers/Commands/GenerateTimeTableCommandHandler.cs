using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Handlers.Commands
{
    public class GenerateTimeTableCommandHandler
        : IRequestHandler<GenerateTimeTableCommand, Result<TimeTableGenerationResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GenerateTimeTableCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TimeTableGenerationResultDto>> Handle(
            GenerateTimeTableCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get Section
                var section = await _unitOfWork.SectionsRepository
                    .GetByIdWithDetailsAsync(request.SectionId, cancellationToken);
                if (section == null)
                    return Result<TimeTableGenerationResultDto>.Failure("Section not found.");

                // 2. Get Subjects
                var subjects = await _unitOfWork.SectionSubjectsRepository
                    .GetBySectionIdAsync(request.SectionId, cancellationToken);
                if (!subjects.Any())
                    return Result<TimeTableGenerationResultDto>.Failure("No subjects mapped to this section.");

                // 3. Get existing timetable entries for this section
                var existingEntries = await _unitOfWork.TimeTablesRepository
                    .GetBySectionIdAsync(request.SectionId, cancellationToken);

                int entriesCreated = 0;
                int entriesSkipped = 0;
                var skippedSubjects = new List<string>();
                var workingDays = new[]
                {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Saturday
                };

                var subjectList = subjects.ToList();
                int subjectIndex = 0;

                // Create a lookup for existing entries by section, day, and period (time slot)
                var existingTimeSlots = existingEntries
                    .Select(e => new { e.DayOfWeek, e.PeriodNumber, e.SubjectId })
                    .ToList();

                foreach (var day in workingDays)
                {
                    for (int period = 1; period <= request.PeriodsPerDay; period++)
                    {
                        if (period == 4) // Skip break period
                            continue;

                        var subject = subjectList[subjectIndex % subjectList.Count];

                        // Check if this time slot (Section + Day + Period) is already occupied
                        var slotOccupied = existingTimeSlots
                            .Any(e => e.DayOfWeek == day && e.PeriodNumber == period);

                        if (slotOccupied)
                        {
                            var existingSubject = existingTimeSlots
                                .FirstOrDefault(e => e.DayOfWeek == day && e.PeriodNumber == period);

                            var subjectName = subjectList.FirstOrDefault(s => s.SubjectId == existingSubject.SubjectId)?.SubjectName ?? "Unknown";

                            if (!skippedSubjects.Contains($"{day} Period {period} - {subjectName}"))
                            {
                                skippedSubjects.Add($"{day} Period {period} - {subjectName}");
                            }
                            entriesSkipped++;
                            subjectIndex++;
                            continue;
                        }

                        var startTime = TimeSpan.FromHours(8) + TimeSpan.FromMinutes((period - 1) * request.PeriodDuration);
                        var endTime = startTime + TimeSpan.FromMinutes(request.PeriodDuration);

                        // Adjust for break time (add break duration after period 3)
                        if (period > 4)
                        {
                            startTime = startTime.Add(TimeSpan.FromMinutes(30)); // Add 30 min break
                            endTime = endTime.Add(TimeSpan.FromMinutes(30));
                        }

                        var entry = new TimeTableEntry(
                            request.SectionId,
                            subject.SubjectId,
                            subject.TeacherId,
                            day,
                            period,
                            startTime,
                            endTime,
                            section.RoomNumber
                        );

                        await _unitOfWork.TimeTablesRepository.AddAsync(entry, cancellationToken);
                        entriesCreated++;
                        subjectIndex++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var response = new TimeTableGenerationResultDto
                {
                    SectionId = request.SectionId,
                    TotalEntriesCreated = entriesCreated,
                    EntriesSkipped = entriesSkipped,
                    SkippedSubjects = skippedSubjects
                };

                var message = $"Successfully created {entriesCreated} timetable entries.";
                if (entriesSkipped > 0)
                {
                    message += $" Skipped {entriesSkipped} time slots that are already occupied.";
                }

                return Result<TimeTableGenerationResultDto>.Success(response, message);
            }
            catch (Exception ex)
            {
                return Result<TimeTableGenerationResultDto>.Failure(
                    "Failed to generate timetable.", ex.Message);
            }
        }
    }
}