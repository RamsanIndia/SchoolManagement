using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Queries
{
    public class GetUserProfileQuery : IRequest<Result<UserDto>>
    {
        public Guid UserId { get; set; }

        public GetUserProfileQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
