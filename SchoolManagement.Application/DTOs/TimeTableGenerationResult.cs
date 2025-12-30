using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    // Domain Service Result (used internally)
    public class TimeTableGenerationResult
    {
        public List<Domain.Entities.TimeTableEntry> NewEntries { get; set; } = new();
        public int EntriesCreated { get; set; }
        public int EntriesSkipped { get; set; }
        public List<SkippedSlotInfo> SkippedSlots { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Map to DTO
        public TimeTableGenerationResultDto ToDto(Guid sectionId)
        {
            return new TimeTableGenerationResultDto
            {
                SectionId = sectionId,
                TotalEntriesCreated = EntriesCreated,
                EntriesSkipped = EntriesSkipped,
                SkippedSlots = SkippedSlots.Select(s => new SkippedSlotDto
                {
                    DayOfWeek = s.DayOfWeek.ToString(),
                    PeriodNumber = s.PeriodNumber,
                    SubjectName = s.SubjectName,
                    Reason = s.Reason
                }).ToList(),
                Warnings = Warnings
            };
        }
    }
}
