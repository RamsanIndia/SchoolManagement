using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TimeTableEntryDto
    {
        public Guid Id { get; set; }
        public int PeriodNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public string RoomNumber { get; set; }
        public Guid SubjectId
        {
            get; set;
        }
    }
}
