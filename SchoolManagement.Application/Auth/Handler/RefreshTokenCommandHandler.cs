// Application/Auth/Handler/RefreshTokenCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IpAddressHelper _ipAddressHelper;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<RefreshTokenCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IpAddressHelper ipAddressHelper
            )
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _ipAddressHelper = ipAddressHelper;
        }

        public async Task<Result<AuthResponseDto>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    _logger.LogWarning("Refresh token request with empty token");
                    return Result<AuthResponseDto>.Failure("Refresh token is required");
                }

                // Get refresh token from repository
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
                    request.RefreshToken,
                    cancellationToken);

                if (refreshToken == null)
                {
                    _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken);
                    return Result<AuthResponseDto>.Failure("Invalid refresh token");
                }

                // Validate token is active using domain logic
                try
                {
                    refreshToken.ValidateActive();
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(
                        "Inactive refresh token used: {TokenId}, Reason: {Reason}",
                        refreshToken.Id,
                        ex.Message);
                    return Result<AuthResponseDto>.Failure("Invalid or expired refresh token");
                }

                // Get user (with all refresh tokens for management)
                var user = await _userRepository.GetByIdWithTokensAsync(
                    refreshToken.UserId,
                    cancellationToken);

                if (user == null)
                {
                    _logger.LogError("User not found for refresh token: {UserId}", refreshToken.UserId);
                    return Result<AuthResponseDto>.Failure("User not found");
                }

                // Check if user account is active
                if (!user.IsActive)
                {
                    _logger.LogWarning(
                        "Refresh token used for inactive user: {UserId}",
                        user.Id);
                    return Result<AuthResponseDto>.Failure("Account is deactivated");
                }

                // Check if account is locked
                if (user.IsLockedOut())
                {
                    _logger.LogWarning(
                        "Refresh token used for locked account: {UserId}",
                        user.Id);
                    return Result<AuthResponseDto>.Failure("Account is locked");
                }

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateAccessToken(user);
                var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
                var clientIp = _ipAddressHelper.GetClientIpAddress();
                // Revoke old token and create new one using domain methods (raises events)
                user.RevokeRefreshToken(
                    refreshToken.Id,
                    "Replaced by new token");

                var newRefreshToken = user.AddRefreshToken(
                    newRefreshTokenValue,
                    DateTime.UtcNow.AddDays(7),
                    clientIp
                    );

                // Update last login
                user.RecordSuccessfulLogin();

                // Clean up expired tokens
                user.RemoveExpiredRefreshTokens();

                // Save changes
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Domain events are automatically dispatched by UnitOfWork
                // Events: RefreshTokenRevoked, RefreshTokenCreated, UserLoggedIn

                _logger.LogInformation(
                    "Successfully refreshed token for user: {UserId}",
                    user.Id);

                // Build response DTO
                var response = new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshTokenValue,
                    User = new UserDto
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email.Value,
                        FirstName = user.FullName.FirstName,
                        LastName = user.FullName.LastName,
                        PhoneNumber = user.PhoneNumber?.Value,
                        IsEmailVerified = user.EmailVerified,
                        IsPhoneVerified = user.PhoneVerified,
                        Roles = new List<string> { user.UserType.ToString() }
                    }
                };

                return Result<AuthResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error refreshing token: {Token}",
                    request.RefreshToken);
                return Result<AuthResponseDto>.Failure(
                    "An error occurred while refreshing token. Please try again.");
            }
        }
    }
}
