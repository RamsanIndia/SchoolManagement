using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Commands
{
    public class UpdateClassCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string ClassCode { get; set; }
        public int Grade { get; set; }
        public string Description { get; set; }
        
    }
}
