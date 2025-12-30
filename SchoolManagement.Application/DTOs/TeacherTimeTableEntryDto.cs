using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TeacherTimeTableEntryDto
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public Guid SubjectId { get; set; }
        public string DayOfWeek { get; set; }
        public int PeriodNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public string RoomNumber { get; set; }
    }
}
