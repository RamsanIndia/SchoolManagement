using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Queries
{
    public class GetTeacherByIdQuery : IRequest<Result<TeacherDto>>
    {
        public Guid TeacherId { get; set; }

        public GetTeacherByIdQuery(Guid teacherId)
        {
            TeacherId = teacherId;
        }
    }
}
