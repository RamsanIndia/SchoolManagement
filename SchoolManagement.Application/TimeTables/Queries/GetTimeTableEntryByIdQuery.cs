using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Queries
{
    public class GetTimeTableEntryByIdQuery : IRequest<Result<object>>
    {
        public Guid Id { get; set; }
    }
}
