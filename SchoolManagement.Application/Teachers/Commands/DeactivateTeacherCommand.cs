using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Commands
{
    public class DeactivateTeacherCommand : IRequest<Result<bool>>
    {
        public Guid TeacherId { get; set; }
        public DateTime? LeavingDate { get; set; }
    }
}
