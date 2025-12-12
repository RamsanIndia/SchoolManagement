using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
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
        public List<SubjectMappingDto> SubjectMappings { get; set; }
    }
}
