using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Auth.Handler;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.UnitTests.Application.Auth
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Setup HttpContext with IP Address
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            //_handler = new LoginCommandHandler(
            //    _mockHttpContextAccessor.Object,
            //    _mockUserRepository.Object,
            //    _mockUnitOfWork.Object,
            //    _mockPasswordService.Object,
            //    _mockTokenService.Object,
            //    _mockLogger.Object);
        }

        #region Successful Login Tests

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsSuccessWithAuthResponse()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "student@school.com",
                Password = "SecurePassword123!"
            };

            var user = CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordService
                .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            //_mockTokenService
            //    .Setup(x => x.GenerateAccessToken(user))
            //    .Returns("access-token-123");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token-456");

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
            result.Status.Should().BeTrue(); // ✅ Changed from IsSuccess to Status
            result.Data.Should().NotBeNull();
            result.Data.AccessToken.Should().Be("access-token-123");
            result.Data.RefreshToken.Should().Be("refresh-token-456");
            result.Data.User.Email.Should().Be(command.Email);

            _mockUserRepository.Verify(
                x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockPasswordService.Verify(
                x => x.VerifyPassword(command.Password, user.PasswordHash),
                Times.Once);

            //_mockTokenService.Verify(
            //    x => x.GenerateAccessToken(user),
            //    Times.Once);

            _mockTokenService.Verify(
                x => x.GenerateRefreshToken(),
                Times.Once);

            _mockUserRepository.Verify(
                x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce());
        }

        #endregion

        #region Failed Login Tests

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "nonexistent@school.com",
                Password = "Password123!"
            };

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().BeFalse(); // ✅ Changed from IsSuccess
            result.Message.Should().Contain("Invalid"); // ✅ Check Message property

            // Note: Errors array might be empty if handler only sets Message
            // If handler sets errors, check: result.Errors.Should().NotBeEmpty();

            _mockPasswordService.Verify(
                x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ReturnsFailureAndIncrementsAttempts()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "student@school.com",
                Password = "WrongPassword!"
            };

            var user = CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordService
                .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(false);

            _mockUserRepository
                .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse(); // ✅ Changed
            result.Message.Should().Contain("Invalid"); // ✅ Changed

            _mockUserRepository.Verify(
                x => x.UpdateAsync(
                    It.Is<User>(u => u.LoginAttempts == 1),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_LockedOutUser_ReturnsFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "locked@school.com",
                Password = "Password123!"
            };

            var user = CreateValidUser();
            // Lock the account
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse(); // ✅ Changed
            result.Message.Should().Contain("locked"); // ✅ Changed

            _mockPasswordService.Verify(
                x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        #endregion

        #region Account Status Tests

        [Fact]
        public async Task Handle_InactiveUser_ReturnsFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "inactive@school.com",
                Password = "Password123!"
            };

            var user = CreateValidUser();
            user.Deactivate("admin", "Account deactivated for testing");

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse(); // ✅ Changed
            result.Message.Should().Contain("deactivated"); // ✅ Changed

            _mockPasswordService.Verify(
                x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        [InlineData("user@")]
        public async Task Handle_InvalidEmailFormat_ReturnsFailure(string invalidEmail)
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = invalidEmail,
                Password = "Password123!"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse(); // ✅ Changed
            result.Message.Should().Contain("Invalid email format"); // ✅ Changed

            _mockUserRepository.Verify(
                x => x.GetByEmailWithLockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region Refresh Token Tests

        [Fact]
        public async Task Handle_SuccessfulLogin_AddsRefreshToken()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "student@school.com",
                Password = "SecurePassword123!"
            };

            var user = CreateValidUser();
            var initialTokenCount = user.RefreshTokens?.Count ?? 0;

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordService
                .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            //_mockTokenService
            //    .Setup(x => x.GenerateAccessToken(user))
            //    .Returns("access-token");

            _mockTokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token-new");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue(); // ✅ Changed
            result.Data.RefreshToken.Should().Be("refresh-token-new");
        }

        #endregion

        #region IP Address Tests

        [Fact]
        public async Task Handle_SuccessfulLogin_RecordsIpAddress()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "student@school.com",
                Password = "SecurePassword123!"
            };

            var user = CreateValidUser();
            var expectedIp = "192.168.1.1";

            _mockUserRepository
                .Setup(x => x.GetByEmailWithLockAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordService
                .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            //_mockTokenService.Setup(x => x.GenerateAccessToken(user)).Returns("token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh");
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue(); // ✅ Changed

            _mockUserRepository.Verify(
                x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region Helper Methods

        private User CreateValidUser()
        {
            var email = new Email("student@school.com");
            var fullName = new FullName("John", "Doe");

            return User.Create(
                new Guid(),
                "johndoe",
                email,
                fullName,
                "hashed-password-123",
                UserType.Student,
                "system",
                "192.168.1.1",
                "test-correlation"
            );
        }

        #endregion
    }
}
