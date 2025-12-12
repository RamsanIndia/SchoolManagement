using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Queries
{
    public class GetTimeTableQuery : IRequest<Result<TimeTableDto>>
    {
        public Guid SectionId { get; set; }
    }
}
