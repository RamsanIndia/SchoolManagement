using MediatR;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await _unitOfWork.AuthRepository.GetRefreshTokenAsync(request.RefreshToken);

            if (refreshToken == null || refreshToken.UserId != request.UserId)
                throw new AuthenticationException("Invalid refresh token");

            refreshToken.Revoke(
                revokedByIp: request.IpAddress ?? "Unknown",
                reason: "User logged out");
            await _unitOfWork.AuthRepository.RevokeRefreshTokenAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
