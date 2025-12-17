using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.RolePermissions.Commands
{
    public class UpdateMenuPermissionCommand : IRequest<Result>
    {
        public Guid RoleId { get; set; }
        public Guid MenuId { get; set; }
        public MenuPermissionsDto Permissions { get; set; }
    }
}
