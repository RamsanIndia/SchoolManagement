using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TimeTableGenerationResultDto
    {
        public Guid SectionId { get; set; }
        public int TotalEntriesCreated { get; set; }
        public int EntriesSkipped { get; set; }
        public List<SkippedSlotDto> SkippedSlots { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
