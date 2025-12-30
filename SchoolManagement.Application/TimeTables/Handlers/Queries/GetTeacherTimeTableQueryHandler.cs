using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GetTeacherTimeTableQueryHandler> _logger;

        private static readonly DayOfWeek[] WorkingDays = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        public GetTeacherTimeTableQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTeacherTimeTableQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<TeacherTimeTableDto>> Handle(
            GetTeacherTimeTableQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving timetable for Teacher {TeacherId}",
                    request.TeacherId);

                // Validate teacher exists
                var teacher = await _unitOfWork.TeachersRepository
                    .GetByIdAsync(request.TeacherId, cancellationToken);

                if (teacher == null)
                {
                    _logger.LogWarning(
                        "Teacher {TeacherId} not found",
                        request.TeacherId);

                    return Result<TeacherTimeTableDto>.Failure(
                        $"Teacher with ID {request.TeacherId} not found.");
                }

                // Get timetable entries
                var entries = await _unitOfWork.TimeTablesRepository
                    .GetByTeacherIdAsync(request.TeacherId, cancellationToken);

                if (!entries.Any())
                {
                    _logger.LogInformation(
                        "No timetable entries found for Teacher {TeacherId}",
                        request.TeacherId);

                    // Return empty schedule instead of failure
                    return Result<TeacherTimeTableDto>.Success(
                        CreateEmptySchedule(request.TeacherId, teacher),
                        "Teacher has no scheduled classes.");
                }

                // Build schedule DTO
                var schedule = BuildSchedule(entries);

                // Calculate statistics
                var statistics = CalculateStatistics(entries);

                var dto = new TeacherTimeTableDto
                {
                    TeacherId = request.TeacherId,
                    TeacherName = teacher.FullName,
                    Email = teacher.Email,
                    Schedule = schedule,
                    Statistics = statistics,
                    TotalPeriodsPerWeek = entries.Count(),
                    LastUpdated = entries.Max(e => e.UpdatedAt ?? e.CreatedAt)
                };

                _logger.LogInformation(
                    "Successfully retrieved timetable for Teacher {TeacherId} with {Count} entries",
                    request.TeacherId,
                    entries.Count());

                return Result<TeacherTimeTableDto>.Success(
                    dto,
                    "Teacher timetable retrieved successfully.");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain error while retrieving timetable for Teacher {TeacherId}: {Message}",
                    request.TeacherId, ex.Message);

                return Result<TeacherTimeTableDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while retrieving timetable for Teacher {TeacherId}",
                    request.TeacherId);

                return Result<TeacherTimeTableDto>.Failure(
                    "Failed to retrieve teacher timetable.");
            }
        }

        private Dictionary<DayOfWeek, List<TeacherTimeTableEntryDto>> BuildSchedule(
            IEnumerable<Domain.Entities.TimeTableEntry> entries)
        {
            var schedule = new Dictionary<DayOfWeek, List<TeacherTimeTableEntryDto>>();

            foreach (var day in WorkingDays)
            {
                var dayEntries = entries
                    .Where(e => e.DayOfWeek == day)
                    .OrderBy(e => e.PeriodNumber)
                    .Select(e => MapToEntryDto(e))
                    .ToList();

                schedule[day] = dayEntries;
            }

            return schedule;
        }

        private TeacherTimeTableEntryDto MapToEntryDto(Domain.Entities.TimeTableEntry entry)
        {
            return new TeacherTimeTableEntryDto
            {
                Id = entry.Id,
                SectionId = entry.SectionId,
                SubjectId = entry.SubjectId,
                DayOfWeek = entry.DayOfWeek.ToString(),
                PeriodNumber = entry.PeriodNumber,
                StartTime = entry.TimePeriod.StartTime,
                EndTime = entry.TimePeriod.EndTime,
                Duration = entry.TimePeriod.Duration,
                ClassName = entry.Section?.Class?.Name ?? "Unknown",
                SectionName = entry.Section?.Name ?? "Unknown",
                RoomNumber = entry.RoomNumber.Value,
                SubjectName = GetSubjectName(entry.SubjectId)
            };
        }

        private string GetSubjectName(Guid subjectId)
        {
            // This could be enhanced to fetch from repository if needed
            // For now, returning a placeholder
            return "Subject"; // TODO: Fetch actual subject name
        }

        private TeacherScheduleStatistics CalculateStatistics(
            IEnumerable<Domain.Entities.TimeTableEntry> entries)
        {
            var entriesList = entries.ToList();

            return new TeacherScheduleStatistics
            {
                TotalPeriodsPerWeek = entriesList.Count,
                PeriodsPerDay = entriesList
                    .GroupBy(e => e.DayOfWeek)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                TotalSections = entriesList.Select(e => e.SectionId).Distinct().Count(),
                TotalSubjects = entriesList.Select(e => e.SubjectId).Distinct().Count(),
                BusiestDay = entriesList
                    .GroupBy(e => e.DayOfWeek)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key.ToString(),
                AveragePeriodsPerDay = Math.Round(
                    entriesList.Count / (double)WorkingDays.Length,
                    2)
            };
        }

        private TeacherTimeTableDto CreateEmptySchedule(
            Guid teacherId,
            Domain.Entities.Teacher teacher)
        {
            var emptySchedule = new Dictionary<DayOfWeek, List<TeacherTimeTableEntryDto>>();

            foreach (var day in WorkingDays)
            {
                emptySchedule[day] = new List<TeacherTimeTableEntryDto>();
            }

            return new TeacherTimeTableDto
            {
                TeacherId = teacherId,
                TeacherName = teacher.FullName,
                Email = teacher.Email,
                Schedule = emptySchedule,
                Statistics = new TeacherScheduleStatistics
                {
                    TotalPeriodsPerWeek = 0,
                    TotalSections = 0,
                    TotalSubjects = 0,
                    AveragePeriodsPerDay = 0
                },
                TotalPeriodsPerWeek = 0,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
