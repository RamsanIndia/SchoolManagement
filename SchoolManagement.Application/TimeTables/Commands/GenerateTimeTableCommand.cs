using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Commands
{
    public class GenerateTimeTableCommand : IRequest<Result<TimeTableGenerationResultDto>>
    {
        public Guid SectionId { get; set; }
        //public DateTime StartDate { get; set; }
        //public DateTime EndDate { get; set; }
        public int PeriodsPerDay { get; set; }
        public int PeriodDuration { get; set; }
        //public bool ConfirmOverwrite { get; set; }
    }
}
