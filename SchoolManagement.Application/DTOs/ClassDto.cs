using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class ClassDto
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string ClassCode { get; set; }
        public int Grade { get; set; }
        public Guid AcademicYearId { get; set; }
        public string Description { get; set; }
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Capacity { get; set; }
    }
}
