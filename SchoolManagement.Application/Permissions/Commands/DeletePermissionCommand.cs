using MediatR;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Permissions.Commands
{
    public class DeletePermissionCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
