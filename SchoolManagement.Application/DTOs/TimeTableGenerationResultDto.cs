using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TimeTableGenerationResultDto
    {
        public int TotalEntriesCreated { get; set; }
        public Guid SectionId { get; set; }
        public int EntriesSkipped { get; set; }
        public List<string> SkippedSubjects { get; set; } = new List<string>();
    }
}
