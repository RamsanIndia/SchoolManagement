// Application/Auth/Commands/RefreshTokenCommand.cs
using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;

namespace SchoolManagement.Application.Auth.Commands
{
    public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
    {
        public string RefreshToken { get; set; }

        public RefreshTokenCommand()
        {
        }

        public RefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
