using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Commands
{
    public class MapSubjectCommand : IRequest<Unit>
    {
        public Guid SectionId { get; set; }
        public Guid TeacherId { get; set; }
        
    }
}
