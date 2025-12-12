using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Queries
{
    public class GetClassByIdQuery : IRequest<Result<ClassDto>>
    {
        public Guid Id { get; set; }
    }
}
