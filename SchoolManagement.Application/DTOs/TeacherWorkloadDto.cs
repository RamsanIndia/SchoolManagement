using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TeacherWorkloadDto
    {
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalWeeklyPeriods { get; set; }
        public int TotalAssignments { get; set; }
        public bool CanAcceptMore { get; set; }
        public int? MaxWeeklyPeriods { get; set; }
        public bool IsActive { get; set; }
        public List<AssignmentDto> Assignments { get; set; } = new();
        public List<ClassTeacherSectionDto> ClassTeacherSections { get; set; } = new();

        // Computed properties
        public int RemainingCapacity => (MaxWeeklyPeriods ?? 40) - TotalWeeklyPeriods;
        public double WorkloadPercentage => MaxWeeklyPeriods.HasValue && MaxWeeklyPeriods > 0
            ? Math.Round((TotalWeeklyPeriods / (double)MaxWeeklyPeriods.Value) * 100, 2)
            : 0;
    }
}
