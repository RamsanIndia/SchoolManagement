using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AcademicYears.Queries
{
    public class GetCurrentAcademicYearQuery : IRequest<Result<AcademicYearDto>>
    {
    }
}
