using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class TimeTableGenerationService : ITimeTableGenerationService
    {
        public TimeTableGenerationResult GenerateTimeTable(
            Section section,
            List<SectionSubject> subjects,
            List<TimeTableEntry> existingEntries,
            TimeTableGenerationOptions options)
        {
            ValidateInput(section, subjects, options);

            var result = new TimeTableGenerationResult();
            var existingSlots = BuildExistingSlotLookup(existingEntries);
            var subjectIndex = 0;

            foreach (var day in options.WorkingDays)
            {
                for (int period = 1; period <= options.PeriodsPerDay; period++)
                {
                    // Skip break period
                    if (period == options.BreakAfterPeriod)
                        continue;

                    // Check if slot is already occupied
                    if (IsSlotOccupied(existingSlots, day, period))
                    {
                        HandleSkippedSlot(result, existingSlots, subjects, day, period);
                        continue;
                    }

                    // Get subject for this period (round-robin distribution)
                    var subject = subjects[subjectIndex % subjects.Count];

                    // Calculate time
                    var (startTime, endTime) = CalculatePeriodTime(
                        period,
                        options.PeriodDuration,
                        options.BreakAfterPeriod,
                        options.BreakDuration,
                        options.SchoolStartTime);

                    try
                    {
                        // Create timetable entry using factory method
                        var entry = TimeTableEntry.Create(
                            section.Id,
                            subject.SubjectId,
                            subject.TeacherId,
                            day,
                            period,
                            startTime,
                            endTime,
                            section.RoomNumber
                        );

                        result.NewEntries.Add(entry);
                        result.EntriesCreated++;
                    }
                    catch (DomainException ex)
                    {
                        result.Warnings.Add(
                            $"Failed to create entry for {day} Period {period}: {ex.Message}");
                    }

                    subjectIndex++;
                }
            }

            // Add warning if subjects are unevenly distributed
            CheckSubjectDistribution(result, subjects, options);

            return result;
        }

        private void ValidateInput(
            Section section,
            List<SectionSubject> subjects,
            TimeTableGenerationOptions options)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            if (subjects == null || !subjects.Any())
                throw new InvalidTimeTableEntryException(
                    "Cannot generate timetable without subjects");

            if (options.PeriodsPerDay <= 0 || options.PeriodsPerDay > 10)
                throw new InvalidPeriodNumberException(options.PeriodsPerDay, 10);

            if (options.PeriodDuration < 30 || options.PeriodDuration > 120)
                throw new InvalidTimePeriodException(
                    $"Period duration must be between 30 and 120 minutes");

            if (string.IsNullOrWhiteSpace(section.RoomNumber))
                throw new InvalidRoomNumberException(
                    section.RoomNumber ?? "null",
                    "Section must have a room number assigned");

            if (options.WorkingDays == null || !options.WorkingDays.Any())
                throw new InvalidDayOfWeekException(
                    "At least one working day must be specified");
        }

        private Dictionary<string, TimeTableEntry> BuildExistingSlotLookup(
            List<TimeTableEntry> existingEntries)
        {
            return existingEntries.ToDictionary(
                e => $"{e.DayOfWeek}_{e.PeriodNumber}",
                e => e);
        }

        private bool IsSlotOccupied(
            Dictionary<string, TimeTableEntry> existingSlots,
            DayOfWeek day,
            int period)
        {
            return existingSlots.ContainsKey($"{day}_{period}");
        }

        private void HandleSkippedSlot(
            TimeTableGenerationResult result,
            Dictionary<string, TimeTableEntry> existingSlots,
            List<SectionSubject> subjects,
            DayOfWeek day,
            int period)
        {
            var existingEntry = existingSlots[$"{day}_{period}"];
            var subject = subjects.FirstOrDefault(s => s.SubjectId == existingEntry.SubjectId);

            result.SkippedSlots.Add(new SkippedSlotInfo
            {
                DayOfWeek = day,
                PeriodNumber = period,
                SubjectName = subject?.SubjectName ?? "Unknown",
                Reason = "Time slot already occupied"
            });

            result.EntriesSkipped++;
        }

        private (TimeSpan startTime, TimeSpan endTime) CalculatePeriodTime(
            int period,
            int periodDuration,
            int breakAfterPeriod,
            int breakDuration,
            TimeSpan schoolStartTime)
        {
            var totalMinutes = (period - 1) * periodDuration;

            // Add break duration if period is after break
            if (period > breakAfterPeriod)
            {
                totalMinutes += breakDuration;
            }

            var startTime = schoolStartTime.Add(TimeSpan.FromMinutes(totalMinutes));
            var endTime = startTime.Add(TimeSpan.FromMinutes(periodDuration));

            return (startTime, endTime);
        }

        private void CheckSubjectDistribution(
            TimeTableGenerationResult result,
            List<SectionSubject> subjects,
            TimeTableGenerationOptions options)
        {
            var totalSlots = options.WorkingDays.Length * (options.PeriodsPerDay - 1); // -1 for break
            var slotsPerSubject = totalSlots / subjects.Count;

            if (totalSlots % subjects.Count != 0)
            {
                result.Warnings.Add(
                    $"Subjects may not be evenly distributed. Total slots: {totalSlots}, " +
                    $"Subjects: {subjects.Count}, Average slots per subject: {slotsPerSubject}");
            }
        }
    }
}
