using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Commands
{
    public class UpdateTeacherProfessionalDetailsCommand : IRequest<Result<bool>>
    {
        public Guid TeacherId { get; set; }
        public string Qualification { get; set; }
        public decimal Experience { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
