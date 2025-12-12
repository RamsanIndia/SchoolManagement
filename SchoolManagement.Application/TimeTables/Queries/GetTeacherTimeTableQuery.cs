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
    public class GetTeacherTimeTableQuery : IRequest<Result<TeacherTimeTableDto>>
    {
        public Guid TeacherId { get; set; }
    }
}
