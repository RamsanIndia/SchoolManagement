using MediatR;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Exceptions;

namespace SchoolManagement.Application.Auth.Handler
{
    public class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, Result<object>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<Result<object>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await _unitOfWork.AuthRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
                return Result<object>.Failure("Invalid refresh token");

            var user = await _unitOfWork.AuthRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
                return Result<object>.Failure("User not found or inactive");

            // Revoke old refresh token
            refreshToken.Revoke();
            await _unitOfWork.AuthRepository.RevokeRefreshTokenAsync(refreshToken);

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken(
                newRefreshTokenValue,
                DateTime.UtcNow.AddDays(7),
                user.Id
            );

            user.AddRefreshToken(newRefreshToken);
            user.RecordLogin();

            await _unitOfWork.AuthRepository.SaveRefreshTokenAsync(newRefreshToken);
            await _unitOfWork.AuthRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            string roleName = Enum.IsDefined(typeof(UserType), user.UserType)
                ? ((UserType)user.UserType).ToString()
                : "Unknown";

            // 👉 Direct object return (NO DTO)
            var response = new
            {
                accessToken = accessToken,
                refreshToken = newRefreshTokenValue,
                user = new
                {
                    id = user.Id.ToString(),
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    roles = new List<string> { roleName }
                }
            };

            return Result<object>.Success(response);
        }
    }
}
