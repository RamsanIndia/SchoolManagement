using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Commands
{
    public class CreateAcademicYearCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
