using MediatR;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Commands
{
    public class MapSubjectCommand : IRequest<Result<Guid>>
    {
        public Guid SectionId { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int WeeklyPeriods { get; set; }
        public bool IsMandatory { get; set; }
        
    }
}
