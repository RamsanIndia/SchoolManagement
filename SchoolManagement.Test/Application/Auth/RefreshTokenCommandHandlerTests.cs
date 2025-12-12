using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Auth.Handler;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SchoolManagement.Test.Application.Auth
{

    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<RefreshTokenCommandHandler>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Setup HttpContext for IP (and forwarded headers if needed)
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Real helper using mocked accessor
            _ipAddressHelper = new IpAddressHelper(_mockHttpContextAccessor.Object);

            _handler = new RefreshTokenCommandHandler(
                _mockUserRepository.Object,          // IUserRepository
                _mockRefreshTokenRepository.Object,  // IRefreshTokenRepository
                _mockUnitOfWork.Object,              // IUnitOfWork
                _mockTokenService.Object,            // ITokenService
                _mockLogger.Object,                  // ILogger<RefreshTokenCommandHandler>
                _mockHttpContextAccessor.Object,     // IHttpContextAccessor
                _ipAddressHelper                     // IpAddressHelper
            );
        }

        #region Successful Token Refresh Tests

        [Fact]
        public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
            var user = CreateValidUser();

            // ✅ FIX: Add token to user and use the same instance for repository mock
            var userRefreshToken = user.AddRefreshToken(
                "valid-refresh-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRefreshToken); // ✅ Use token from user's collection

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _mockUserRepository
                .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.AccessToken.Should().Be("new-access-token");
            result.Data.RefreshToken.Should().Be("new-refresh-token");
            result.Data.User.Should().NotBeNull();
            result.Data.User.Email.Should().Be("test@school.com");

            _mockRefreshTokenRepository.Verify(
                x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUserRepository.Verify(
                x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockTokenService.Verify(
                x => x.GenerateAccessToken(user),
                Times.Once);

            _mockTokenService.Verify(
                x => x.GenerateRefreshToken(),
                Times.Once);

            _mockUserRepository.Verify(
                x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRefreshToken_RevokesOldToken()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
            var user = CreateValidUser();

            // ✅ FIX: Add token and use same instance
            var userRefreshToken = user.AddRefreshToken(
                "valid-refresh-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRefreshToken); // ✅ Use same token

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();

            // Verify old token was revoked
            _mockUserRepository.Verify(
                x => x.UpdateAsync(
                    It.Is<User>(u => u.RefreshTokens.Any(t => t.RevokedAt != null)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRefreshToken_UpdatesLastLogin()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
            var user = CreateValidUser();

            // ✅ FIX: Add token and use same instance
            var userRefreshToken = user.AddRefreshToken(
                "valid-refresh-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRefreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();

            _mockUserRepository.Verify(
                x => x.UpdateAsync(
                    It.Is<User>(u => u.LastLoginAt != null),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRefreshToken_ProcessesSuccessfullyWithMultipleTokens()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
            var user = CreateValidUser();

            // Add multiple tokens from different devices
            user.AddRefreshToken("other-device-token-1", DateTime.UtcNow.AddDays(5), "192.168.1.2");
            user.AddRefreshToken("other-device-token-2", DateTime.UtcNow.AddDays(3), "192.168.1.3");

            // ✅ FIX: Add current token and use same instance
            var userRefreshToken = user.AddRefreshToken(
                "valid-refresh-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRefreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            result.Data.AccessToken.Should().Be("new-access-token");
            result.Data.RefreshToken.Should().Be("new-refresh-token");

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region Invalid Token Tests

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task Handle_EmptyRefreshToken_ReturnsFailure(string emptyToken)
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = emptyToken };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("Refresh token is required");

            _mockRefreshTokenRepository.Verify(
                x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_TokenNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "non-existent-token" };

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("Invalid refresh token");

            _mockUserRepository.Verify(
                x => x.GetByIdWithTokensAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_RevokedToken_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "revoked-token" };
            var user = CreateValidUser();
            var revokedToken = CreateRevokedRefreshToken(user.Id);

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(revokedToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("Invalid or expired refresh token");
        }

        #endregion

        #region User Account Status Tests

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-token" };
            var userId = Guid.NewGuid();
            var refreshToken = CreateValidRefreshToken(userId);

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("User not found");

            _mockTokenService.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InactiveUser_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-token" };
            var user = CreateValidUser();
            user.Deactivate("admin", "Account deactivated");
            var refreshToken = CreateValidRefreshToken(user.Id);

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("deactivated");

            _mockTokenService.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_LockedUser_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-token" };
            var user = CreateValidUser();

            // Lock the account
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            var refreshToken = CreateValidRefreshToken(user.Id);

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("locked");

            _mockTokenService.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-token" };

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("error occurred");
            result.Message.Should().Contain("try again");
        }

        [Fact]
        public async Task Handle_TokenServiceException_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-token" };
            var user = CreateValidUser();

            var userRefreshToken = user.AddRefreshToken(
                "valid-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRefreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Throws(new Exception("Token generation failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("error occurred");
        }

        [Fact]
        public async Task Handle_SaveChangesException_ReturnsFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "valid-token" };
            var user = CreateValidUser();

            var userRefreshToken = user.AddRefreshToken(
                "valid-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            _mockRefreshTokenRepository
                .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRefreshToken);

            _mockUserRepository
                .Setup(x => x.GetByIdWithTokensAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database save failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("error occurred");
        }

        #endregion

        #region Helper Methods

        private User CreateValidUser()
        {
            var email = new Email("test@school.com");
            var fullName = new FullName("Test", "User");

            return User.Create(
                "testuser",
                email,
                fullName,
                "hashed-password",
                UserType.Student,
                "System",
                "192.168.1.1",
                Guid.NewGuid().ToString());
        }

        private RefreshToken CreateValidRefreshToken(Guid userId)
        {
            return RefreshToken.Create(
                userId,
                "valid-refresh-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");
        }

        private RefreshToken CreateRevokedRefreshToken(Guid userId)
        {
            var token = RefreshToken.Create(
                userId,
                "revoked-token",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1");

            token.Revoke("192.168.1.1", "Token revoked");
            return token;
        }

        #endregion
    }
}
