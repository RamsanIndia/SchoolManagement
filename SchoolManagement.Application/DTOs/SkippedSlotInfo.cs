using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class SkippedSlotInfo
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int PeriodNumber { get; set; }
        public string SubjectName { get; set; }
        public string Reason { get; set; }
    }
}
