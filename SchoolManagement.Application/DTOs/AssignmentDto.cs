using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class AssignmentDto
    {
        public Guid SectionId { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public int WeeklyPeriods { get; set; }
        public bool IsMandatory { get; set; }
    }
}
