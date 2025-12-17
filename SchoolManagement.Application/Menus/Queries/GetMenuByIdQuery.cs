using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Menus.Queries
{
    public class GetMenuByIdQuery : IRequest<Result<MenuDto>>
    {
        public Guid Id { get; set; }
    }

}
