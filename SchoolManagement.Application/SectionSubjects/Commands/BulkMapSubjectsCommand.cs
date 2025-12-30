using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Commands
{
    public class BulkMapSubjectsCommand : IRequest<Result<BulkMapResult>>
    {
        public Guid SectionId { get; set; }
        public List<SubjectMappingDto> SubjectMappings { get; set; } = new();
    }

    public class SubjectMappingDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int WeeklyPeriods { get; set; }
        public bool IsMandatory { get; set; }
    }
}
