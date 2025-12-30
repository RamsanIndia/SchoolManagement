using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TeacherDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeId { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public string Qualification { get; set; }
        public decimal Experience { get; set; }
        public int TotalExperience { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }

        // Address
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Workload info
        public int TotalTeachingAssignments { get; set; }
        public int TotalWeeklyPeriods { get; set; }
        public bool IsSenior { get; set; }
    }
}
