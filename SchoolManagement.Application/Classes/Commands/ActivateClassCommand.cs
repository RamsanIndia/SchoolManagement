using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Commands
{
    public class ActivateClassCommand : IRequest<Result>
    {
        public Guid ClassId { get; set; }
        
    }
}
