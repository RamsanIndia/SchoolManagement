using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TeacherTimeTableDto
    {
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public Dictionary<DayOfWeek, List<TeacherTimeTableEntryDto>> Schedule { get; set; }
    }
}
