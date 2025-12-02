using MediatR;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Commands
{
    public class AssignClassTeacherCommand:IRequest<Result<bool>>
    {
        public Guid SectionId { get; set; }
        public Guid TeacherId { get; set; } = Guid.Empty;
    }
}
