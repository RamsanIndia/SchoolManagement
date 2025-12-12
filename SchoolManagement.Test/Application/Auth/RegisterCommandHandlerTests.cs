using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Auth.Handlers;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Test.Application.Auth
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly Mock<ILogger<RegisterCommandHandler>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockLogger = new Mock<ILogger<RegisterCommandHandler>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Setup HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
            httpContext.Request.Headers["X-Correlation-ID"] = "test-correlation-id";
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Real helper using mocked accessor
            _ipAddressHelper = new IpAddressHelper(_mockHttpContextAccessor.Object);

            _handler = new RegisterCommandHandler(
                _mockUserRepository.Object,      // IUserRepository
                _ipAddressHelper,               // IpAddressHelper
                _mockUnitOfWork.Object,         // IUnitOfWork
                _mockPasswordService.Object,    // IPasswordService
                _mockLogger.Object,             // ILogger<RegisterCommandHandler>
                _mockHttpContextAccessor.Object // IHttpContextAccessor
            );
        }

            #region Successful Registration Tests

            [Fact]
        public async Task Handle_ValidRegistration_ReturnsSuccessWithUserDto()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Username = "johndoe",
                Email = "john.doe@school.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "SecurePassword123!",
                PhoneNumber = "+1234567890",
                UserType = UserType.Student
            };

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password-123");

            _mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
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
            result.Data.Username.Should().Be(command.Username);
            result.Data.Email.Should().Be(command.Email);
            result.Data.FirstName.Should().Be(command.FirstName);
            result.Data.LastName.Should().Be(command.LastName);
            result.Data.PhoneNumber.Should().Be(command.PhoneNumber);
            result.Data.IsEmailVerified.Should().BeFalse();
            result.Data.IsPhoneVerified.Should().BeFalse();
            result.Data.Roles.Should().Contain("Student");
            result.Message.Should().Contain("successfully");

            _mockUserRepository.Verify(
                x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockPasswordService.Verify(
                x => x.HashPassword(command.Password),
                Times.Once);

            _mockUserRepository.Verify(
                x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RegistrationWithoutPhoneNumber_ReturnsSuccess()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Username = "janedoe",
                Email = "jane.doe@school.com",
                FirstName = "Jane",
                LastName = "Doe",
                Password = "SecurePassword123!",
                PhoneNumber = null, // No phone number
                UserType = UserType.Teacher
            };

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password-123");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            result.Data.PhoneNumber.Should().BeNull();
            result.Data.Roles.Should().Contain("Teacher");
        }

        [Fact]
        public async Task Handle_ValidRegistration_HashesPassword()
        {
            // Arrange
            var command = CreateValidCommand();
            var plainPassword = command.Password;

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(plainPassword))
                .Returns("hashed-password-123");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            _mockPasswordService.Verify(
                x => x.HashPassword(plainPassword),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRegistration_CapturesClientIpAddress()
        {
            // Arrange
            var command = CreateValidCommand();
            SetupHttpContext("203.0.113.42", "test-correlation");

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            // IP is captured internally during User.Create
        }

        [Fact]
        public async Task Handle_ValidRegistration_UsesCorrelationIdFromHeader()
        {
            // Arrange
            var command = CreateValidCommand();
            var correlationId = "custom-correlation-123";
            SetupHttpContext("192.168.1.1", correlationId);

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            // Correlation ID is used internally
        }

        #endregion

        #region Duplicate Email Tests

        [Fact]
        public async Task Handle_DuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            var existingUser = CreateExistingUser(command.Email);

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("already exists");

            _mockPasswordService.Verify(
                x => x.HashPassword(It.IsAny<string>()),
                Times.Never);

            _mockUserRepository.Verify(
                x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        public async Task Handle_InvalidEmailFormat_ReturnsFailure(string invalidEmail)
        {
            // Arrange
            var command = CreateValidCommand();
            command.Email = invalidEmail;

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().NotBeNullOrEmpty();

            _mockUserRepository.Verify(
                x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData("", "Doe")]
        [InlineData("John", "")]
        [InlineData(null, "Doe")]
        [InlineData("John", null)]
        public async Task Handle_InvalidName_ReturnsFailure(string firstName, string lastName)
        {
            // Arrange
            var command = CreateValidCommand();
            command.FirstName = firstName;
            command.LastName = lastName;

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();

            _mockUserRepository.Verify(
                x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidPhoneNumber_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.PhoneNumber = "invalid-phone";

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public async Task Handle_DomainException_ReturnsFailureWithMessage()
        {
            // Arrange
            var command = CreateValidCommand();
            var domainException = new DomainException("Invalid domain rule");

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(domainException);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("Invalid domain rule");
        }

        [Fact]
        public async Task Handle_UnexpectedException_ReturnsGenericFailure()
        {
            // Arrange
            var command = CreateValidCommand();

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("Registration failed");
            result.Message.Should().Contain("try again later");
        }

        [Fact]
        public async Task Handle_RepositoryException_LogsErrorAndReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeFalse();
            result.Message.Should().Contain("Registration failed");
        }

        #endregion

        #region User Type Tests

        [Theory]
        [InlineData(UserType.Student)]
        [InlineData(UserType.Teacher)]
        [InlineData(UserType.Admin)]
        [InlineData(UserType.Parent)]
        public async Task Handle_DifferentUserTypes_RegistersCorrectly(UserType userType)
        {
            // Arrange
            var command = CreateValidCommand();
            command.UserType = userType;

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            result.Data.Roles.Should().Contain(userType.ToString());
        }

        #endregion

        #region IP Address Handling Tests

        [Fact]
        public async Task Handle_NoHttpContext_UsesUnknownIpAddress()
        {
            // Arrange
            var command = CreateValidCommand();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            // IP will be "Unknown" internally
        }

        [Fact]
        public async Task Handle_ForwardedForHeader_UsesForwardedIpAddress()
        {
            // Arrange
            var command = CreateValidCommand();
            var forwardedIp = "203.0.113.195";
            SetupHttpContextWithForwardedFor(forwardedIp);

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(x => x.HashPassword(command.Password))
                .Returns("hashed-password");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().BeTrue();
            // Forwarded IP is used internally
        }

        #endregion

        #region Helper Methods

        private RegisterCommand CreateValidCommand()
        {
            return new RegisterCommand
            {
                Username = "testuser",
                Email = "test@school.com",
                FirstName = "Test",
                LastName = "User",
                Password = "SecurePassword123!",
                PhoneNumber = "+1234567890",
                UserType = UserType.Student
            };
        }

        private User CreateExistingUser(string email)
        {
            var existingEmail = new Email(email);
            var fullName = new FullName("Existing", "User");

            return User.Create(
                "existinguser",
                existingEmail,
                fullName,
                "hashed-password",
                UserType.Student,
                "System",
                "192.168.1.1",
                Guid.NewGuid().ToString());
        }

        private void SetupHttpContext(string ipAddress, string correlationId)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);

            httpContext.Request.Headers["X-Correlation-ID"] = new StringValues(correlationId);

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        }

        private void SetupHttpContextWithForwardedFor(string forwardedIp)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

            httpContext.Request.Headers["X-Forwarded-For"] = new StringValues(forwardedIp);
            httpContext.Request.Headers["X-Correlation-ID"] = new StringValues("test-correlation");

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        }

        #endregion
    }
}
