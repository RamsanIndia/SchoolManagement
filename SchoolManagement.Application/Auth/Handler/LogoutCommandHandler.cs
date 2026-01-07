using MediatR;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly IpAddressHelper _ipAddressHelper;

        public LogoutCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IpAddressHelper ipAddressHelper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ipAddressHelper = ipAddressHelper;
        }

        public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await _unitOfWork.AuthRepository.GetRefreshTokenAsync(request.RefreshToken);

            if (refreshToken == null || refreshToken.UserId != request.UserId)
                throw new AuthenticationException("Invalid refresh token");

            refreshToken.Revoke(
                revokedByIp: _ipAddressHelper.GetIpAddress() ?? "Unknown",
                revokedBy: _currentUserService.Username,
                reason: "User logged out");
            await _unitOfWork.AuthRepository.RevokeRefreshTokenAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
