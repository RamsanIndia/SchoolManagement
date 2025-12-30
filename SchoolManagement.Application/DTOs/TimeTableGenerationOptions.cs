using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TimeTableGenerationOptions
    {
        public int PeriodsPerDay { get; set; }
        public int PeriodDuration { get; set; }
        public int BreakAfterPeriod { get; set; }
        public int BreakDuration { get; set; }
        public TimeSpan SchoolStartTime { get; set; }
        public bool OverwriteExisting { get; set; }
        public DayOfWeek[] WorkingDays { get; set; }
    }
}
