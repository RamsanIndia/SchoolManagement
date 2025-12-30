using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.TimeTables.Commands
{
    public class GenerateTimeTableCommand : IRequest<Result<TimeTableGenerationResultDto>>
    {
        public Guid SectionId { get; set; }
        public int PeriodsPerDay { get; set; } = 8;
        public int PeriodDuration { get; set; } = 45; // minutes
        public int BreakAfterPeriod { get; set; } = 4;
        public int BreakDuration { get; set; } = 30; // minutes
        public TimeSpan SchoolStartTime { get; set; } = TimeSpan.FromHours(8); // 8:00 AM
        public bool OverwriteExisting { get; set; } = false;
        public DayOfWeek[] WorkingDays { get; set; } = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        public GenerateTimeTableCommand()
        {
        }

        public GenerateTimeTableCommand(
            Guid sectionId,
            int periodsPerDay = 8,
            int periodDuration = 45,
            int breakAfterPeriod = 4,
            int breakDuration = 30,
            TimeSpan? schoolStartTime = null,
            bool overwriteExisting = false,
            DayOfWeek[] workingDays = null)
        {
            SectionId = sectionId;
            PeriodsPerDay = periodsPerDay;
            PeriodDuration = periodDuration;
            BreakAfterPeriod = breakAfterPeriod;
            BreakDuration = breakDuration;
            SchoolStartTime = schoolStartTime ?? TimeSpan.FromHours(8);
            OverwriteExisting = overwriteExisting;
            WorkingDays = workingDays ?? new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            };
        }
    }
}
