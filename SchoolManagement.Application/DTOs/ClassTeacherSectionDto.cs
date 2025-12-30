using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class ClassTeacherSectionDto
    {
        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public int CurrentStrength { get; set; }
        public int MaxCapacity { get; set; }
    }
}
