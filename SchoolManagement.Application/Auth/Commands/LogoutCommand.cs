// Application/Auth/Commands/LogoutCommand.cs
using MediatR;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Auth.Commands
{
    public class LogoutCommand : IRequest<Result<bool>>
    {
        public string RefreshToken { get; set; }
        public Guid UserId { get; set; }
        public string IpAddress { get; set; }

        public LogoutCommand()
        {
        }

        public LogoutCommand(string refreshToken, Guid userId, string ipAddress = null)
        {
            RefreshToken = refreshToken;
            UserId = userId;
            IpAddress = ipAddress;
        }
    }
}
