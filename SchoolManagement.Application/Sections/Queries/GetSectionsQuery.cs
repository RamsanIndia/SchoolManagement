using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Queries
{
    public class GetSectionsQuery : IRequest<Result<List<SectionDto>>>
    {
        public Guid? ClassId { get; set; }
    }
}
