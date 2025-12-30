using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Commands
{
    public class DeleteSectionTimeTableCommand : IRequest<Result<int>>
    {
        public Guid SectionId { get; set; }
    }
}
