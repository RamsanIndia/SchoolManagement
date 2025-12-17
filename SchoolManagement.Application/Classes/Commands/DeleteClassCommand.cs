using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Commands
{
    public class DeleteClassCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        
    }
}
