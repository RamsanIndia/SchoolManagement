using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Commands
{
    public class DeactivateAcademicYearCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}
