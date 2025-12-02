using MediatR;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Commands
{
    public class CreateSectionCommand : IRequest<Result<Guid>>
    {
        public Guid ClassId { get; set; }
        public string SectionName { get; set; }
        public int Capacity { get; set; }
        public string RoomNumber { get; set; }
        
    }
}
