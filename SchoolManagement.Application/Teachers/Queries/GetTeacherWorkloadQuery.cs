using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Teachers.Queries
{
    public class GetTeacherWorkloadQuery : IRequest<Result<TeacherWorkloadDto>>
    {
        public Guid TeacherId { get; set; }

        // Parameterless constructor
        public GetTeacherWorkloadQuery()
        {
        }

        // Constructor
        public GetTeacherWorkloadQuery(Guid teacherId)
        {
            if (teacherId == Guid.Empty)
                throw new ArgumentException("Teacher ID cannot be empty", nameof(teacherId));

            TeacherId = teacherId;
        }
    }
}
