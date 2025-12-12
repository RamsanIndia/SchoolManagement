using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TimeTableDto
    {
        public Guid SectionId { get; set; }
        public string SectionName { get; set; }
        public string ClassName { get; set; }
        public Dictionary<DayOfWeek, List<TimeTableEntryDto>> Schedule { get; set; }
    }
}
