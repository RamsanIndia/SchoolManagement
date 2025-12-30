using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TeacherScheduleStatistics
    {
        public int TotalPeriodsPerWeek { get; set; }
        public Dictionary<string, int> PeriodsPerDay { get; set; }
        public int TotalSections { get; set; }
        public int TotalSubjects { get; set; }
        public string BusiestDay { get; set; }
        public double AveragePeriodsPerDay { get; set; }
    }
}
