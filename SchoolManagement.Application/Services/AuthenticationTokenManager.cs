using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class AuthenticationTokenManager : IAuthenticationTokenManager
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthenticationTokenManager> _logger;

        public AuthenticationTokenManager(
            ITokenService tokenService,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<AuthenticationTokenManager> logger)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TokenDto> GenerateTokensAsync(
            User user,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("🔑 Generated tokens for user: {UserId}", user.Id);
            _logger.LogInformation("🔍 BEFORE - RefreshTokens count: {Count}", user.RefreshTokens.Count);

            // Clean up expired tokens
            user.RemoveExpiredRefreshTokens();
            _logger.LogInformation("🧹 After cleanup - RefreshTokens count: {Count}", user.RefreshTokens.Count);

            // Add new refresh token
            var newToken = user.AddRefreshToken(
                refreshTokenValue,
                DateTime.UtcNow.AddDays(7),
                
                ipAddress
            );

            _logger.LogInformation("➕ Added token - ID: {TokenId}, UserId: {UserId}", newToken.Id, newToken.UserId);
            _logger.LogInformation("🔍 AFTER adding - RefreshTokens count: {Count}", user.RefreshTokens.Count);

            // Record successful login
            user.RecordSuccessfulLogin();

            // FIX: Explicitly add token for tracking
            await _unitOfWork.AddRefreshTokenAsync(newToken, cancellationToken);

            // Save changes
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("💾 Calling SaveChangesAsync...");
            var savedCount = await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("✅ SaveChangesAsync completed. Saved {SavedCount} changes.", savedCount);

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue
            };
        }
    }
}
