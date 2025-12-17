using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Commands
{
    public class UpdateSubjectMappingCommand : IRequest<Result<bool>>
    {
        public Guid MappingId { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int WeeklyPeriods { get; set; }
    }
}
