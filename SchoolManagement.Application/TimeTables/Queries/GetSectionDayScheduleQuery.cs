using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Queries
{
    public class GetSectionDayScheduleQuery : IRequest<Result<object>>
    {
        public Guid SectionId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }
}
