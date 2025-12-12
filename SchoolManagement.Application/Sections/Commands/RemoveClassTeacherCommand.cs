using MediatR;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Commands
{
    public class RemoveClassTeacherCommand : IRequest<Result>
    {
        public Guid SectionId { get; set; }
    }
}
