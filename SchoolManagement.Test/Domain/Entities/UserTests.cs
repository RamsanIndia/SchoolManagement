using FluentAssertions;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Test.Domain.Entities
{
    public class UserTests
    {
        private const string TestUsername = "johndoe";
        private const string TestPassword = "hashed-password-123";
        private const string CreatedBy = "system";
        private const string CreatedIp = "192.168.1.1";
        private const string CorrelationId = "test-correlation-id";

        #region Factory Method Tests

        [Fact]
        public void Create_ValidParameters_ReturnsUser()
        {
            // Arrange
            var email = new Email("student@school.com");
            var fullName = new FullName("John", "Doe");

            // Act
            var user = User.Create(
                TestUsername,
                email,
                fullName,
                TestPassword,
                UserType.Student,
                CreatedBy,
                CreatedIp,
                CorrelationId);

            // Assert
            user.Should().NotBeNull();
            user.Username.Should().Be(TestUsername);
            user.Email.Should().Be(email);
            user.FullName.Should().Be(fullName);
            user.PasswordHash.Should().Be(TestPassword);
            user.UserType.Should().Be(UserType.Student);
            user.IsActive.Should().BeTrue();
            user.EmailVerified.Should().BeFalse();
            user.PhoneVerified.Should().BeFalse();
            user.LoginAttempts.Should().Be(0);
        }

        [Fact]
        public void Create_ValidParameters_RaisesUserCreatedEvent()
        {
            // Arrange
            var email = new Email("student@school.com");
            var fullName = new FullName("John", "Doe");

            // Act
            var user = User.Create(
                TestUsername,
                email,
                fullName,
                TestPassword,
                UserType.Student,
                CreatedBy,
                CreatedIp,
                CorrelationId);

            // Assert
            var domainEvent = user.DomainEvents.OfType<UserCreatedEvent>().FirstOrDefault();
            domainEvent.Should().NotBeNull();
            domainEvent.UserId.Should().Be(user.Id);
            domainEvent.Username.Should().Be(TestUsername);
            domainEvent.Email.Should().Be(email.Value);
            domainEvent.UserType.Should().Be(UserType.Student);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Create_InvalidUsername_ThrowsArgumentException(string invalidUsername)
        {
            // Arrange
            var email = new Email("student@school.com");
            var fullName = new FullName("John", "Doe");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                User.Create(
                    invalidUsername,
                    email,
                    fullName,
                    TestPassword,
                    UserType.Student,
                    CreatedBy,
                    CreatedIp,
                    CorrelationId));

            exception.ParamName.Should().Be("username");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Create_InvalidPasswordHash_ThrowsArgumentException(string invalidPassword)
        {
            // Arrange
            var email = new Email("student@school.com");
            var fullName = new FullName("John", "Doe");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                User.Create(
                    TestUsername,
                    email,
                    fullName,
                    invalidPassword,
                    UserType.Student,
                    CreatedBy,
                    CreatedIp,
                    CorrelationId));

            exception.ParamName.Should().Be("passwordHash");
        }

        [Fact]
        public void Create_NullEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var fullName = new FullName("John", "Doe");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                User.Create(
                    TestUsername,
                    null,
                    fullName,
                    TestPassword,
                    UserType.Student,
                    CreatedBy,
                    CreatedIp,
                    CorrelationId));
        }

        [Fact]
        public void Create_NullFullName_ThrowsArgumentNullException()
        {
            // Arrange
            var email = new Email("student@school.com");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                User.Create(
                    TestUsername,
                    email,
                    null,
                    TestPassword,
                    UserType.Student,
                    CreatedBy,
                    CreatedIp,
                    CorrelationId));
        }

        #endregion

        #region Password Management Tests

        [Fact]
        public void UpdatePassword_ValidPassword_UpdatesPasswordHash()
        {
            // Arrange
            var user = CreateTestUser();
            var newPasswordHash = "new-hashed-password";

            // Act
            user.UpdatePassword(newPasswordHash, "admin");

            // Assert
            user.PasswordHash.Should().Be(newPasswordHash);
        }

        [Fact]
        public void UpdatePassword_ValidPassword_RaisesPasswordChangedEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.UpdatePassword("new-password-hash", "admin");

            // Assert
            var domainEvent = user.DomainEvents.OfType<UserPasswordChangedEvent>().FirstOrDefault();
            domainEvent.Should().NotBeNull();
            domainEvent.UserId.Should().Be(user.Id);
            domainEvent.Username.Should().Be(user.Username);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void UpdatePassword_InvalidPassword_ThrowsArgumentException(string invalidPassword)
        {
            // Arrange
            var user = CreateTestUser();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                user.UpdatePassword(invalidPassword, "admin"));
        }

        #endregion

        #region Profile Management Tests

        [Fact]
        public void UpdateProfile_ValidData_UpdatesProfile()
        {
            // Arrange
            var user = CreateTestUser();
            var newFullName = new FullName("Jane", "Smith");
            var newEmail = new Email("jane.smith@school.com");

            // Act
            user.UpdateProfile(newFullName, newEmail, "admin");

            // Assert
            user.FullName.Should().Be(newFullName);
            user.Email.Should().Be(newEmail);
        }

        [Fact]
        public void UpdateProfile_EmailChanged_SetsEmailVerifiedToFalse()
        {
            // Arrange
            var user = CreateTestUser();
            user.VerifyEmail("system");
            user.EmailVerified.Should().BeTrue();

            var newEmail = new Email("newemail@school.com");

            // Act
            user.UpdateProfile(user.FullName, newEmail, "admin");

            // Assert
            user.EmailVerified.Should().BeFalse();
        }

        [Fact]
        public void UpdateProfile_EmailChanged_RaisesEmailChangedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            var newEmail = new Email("newemail@school.com");

            // Act
            user.UpdateProfile(user.FullName, newEmail, "admin");

            // Assert
            var emailChangedEvent = user.DomainEvents.OfType<UserEmailChangedEvent>().FirstOrDefault();
            emailChangedEvent.Should().NotBeNull();
            emailChangedEvent.NewEmail.Should().Be(newEmail.Value);
        }

        [Fact]
        public void UpdateProfile_EmailUnchanged_DoesNotRaiseEmailChangedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            var sameEmail = user.Email;

            // Act
            user.UpdateProfile(user.FullName, sameEmail, "admin");

            // Assert
            var emailChangedEvent = user.DomainEvents.OfType<UserEmailChangedEvent>().FirstOrDefault();
            emailChangedEvent.Should().BeNull();
        }

        [Fact]
        public void UpdateProfile_ValidData_RaisesProfileUpdatedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            var newFullName = new FullName("Jane", "Smith");

            // Act
            user.UpdateProfile(newFullName, user.Email, "admin");

            // Assert
            var profileUpdatedEvent = user.DomainEvents.OfType<UserProfileUpdatedEvent>().FirstOrDefault();
            profileUpdatedEvent.Should().NotBeNull();
            profileUpdatedEvent.FirstName.Should().Be("Jane");
            profileUpdatedEvent.LastName.Should().Be("Smith");
        }

        [Fact]
        public void UpdatePhoneNumber_NewPhoneNumber_UpdatesAndSetsUnverified()
        {
            // Arrange
            var user = CreateTestUser();
            var newPhoneNumber = new PhoneNumber("+9876543210");

            // Act
            user.UpdatePhoneNumber(newPhoneNumber, "admin");

            // Assert
            user.PhoneNumber.Should().Be(newPhoneNumber);
            user.PhoneVerified.Should().BeFalse();
        }

        [Fact]
        public void UpdatePhoneNumber_PhoneChanged_RaisesPhoneNumberChangedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            var newPhoneNumber = new PhoneNumber("+9876543210");

            // Act
            user.UpdatePhoneNumber(newPhoneNumber, "admin");

            // Assert
            var phoneChangedEvent = user.DomainEvents.OfType<UserPhoneNumberChangedEvent>().FirstOrDefault();
            phoneChangedEvent.Should().NotBeNull();
            phoneChangedEvent.NewPhoneNumber.Should().Be(newPhoneNumber.Value);
        }

        #endregion

        #region Account Status Management Tests

        [Fact]
        public void Activate_InactiveUser_ActivatesUser()
        {
            // Arrange
            var user = CreateTestUser();
            user.Deactivate("admin");
            user.IsActive.Should().BeFalse();

            // Act
            user.Activate("admin");

            // Assert
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Activate_InactiveUser_RaisesUserActivatedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            user.Deactivate("admin");

            // Act
            user.Activate("admin");

            // Assert
            var activatedEvent = user.DomainEvents.OfType<UserActivatedEvent>().LastOrDefault();
            activatedEvent.Should().NotBeNull();
            activatedEvent.UserId.Should().Be(user.Id);
        }

        [Fact]
        public void Activate_AlreadyActiveUser_DoesNothing()
        {
            // Arrange
            var user = CreateTestUser();
            var initialEventCount = user.DomainEvents.Count;

            // Act
            user.Activate("admin");

            // Assert
            user.IsActive.Should().BeTrue();
            user.DomainEvents.Count.Should().Be(initialEventCount); // No new events
        }

        [Fact]
        public void Deactivate_ActiveUser_DeactivatesUser()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.Deactivate("admin", "User requested account deletion");

            // Assert
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Deactivate_ActiveUser_RaisesUserDeactivatedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            var reason = "Policy violation";

            // Act
            user.Deactivate("admin", reason);

            // Assert
            var deactivatedEvent = user.DomainEvents.OfType<UserDeactivatedEvent>().FirstOrDefault();
            deactivatedEvent.Should().NotBeNull();
            deactivatedEvent.Reason.Should().Be(reason);
        }

        #endregion

        #region Email and Phone Verification Tests

        [Fact]
        public void VerifyEmail_UnverifiedEmail_VerifiesEmail()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.VerifyEmail("system");

            // Assert
            user.EmailVerified.Should().BeTrue();
        }

        [Fact]
        public void VerifyEmail_UnverifiedEmail_RaisesEmailVerifiedEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.VerifyEmail("system");

            // Assert
            var verifiedEvent = user.DomainEvents.OfType<UserEmailVerifiedEvent>().FirstOrDefault();
            verifiedEvent.Should().NotBeNull();
            verifiedEvent.Email.Should().Be(user.Email.Value);
        }

        [Fact]
        public void VerifyEmail_AlreadyVerified_DoesNothing()
        {
            // Arrange
            var user = CreateTestUser();
            user.VerifyEmail("system");
            var initialEventCount = user.DomainEvents.Count;

            // Act
            user.VerifyEmail("system");

            // Assert
            user.DomainEvents.Count.Should().Be(initialEventCount);
        }

        [Fact]
        public void VerifyPhone_UnverifiedPhone_VerifiesPhone()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.VerifyPhone("system");

            // Assert
            user.PhoneVerified.Should().BeTrue();
        }

        [Fact]
        public void VerifyPhone_UnverifiedPhone_RaisesPhoneVerifiedEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.VerifyPhone("system");

            // Assert
            var verifiedEvent = user.DomainEvents.OfType<UserPhoneVerifiedEvent>().FirstOrDefault();
            verifiedEvent.Should().NotBeNull();
        }

        #endregion

        #region User Linking Tests

        [Fact]
        public void LinkToStudent_StudentUserType_LinksSuccessfully()
        {
            // Arrange
            var user = CreateTestUser(UserType.Student);
            var studentId = Guid.NewGuid();

            // Act
            user.LinkToStudent(studentId, "admin");

            // Assert
            user.StudentId.Should().Be(studentId);
        }

        [Fact]
        public void LinkToStudent_ParentUserType_LinksSuccessfully()
        {
            // Arrange
            var user = CreateTestUser(UserType.Parent);
            var studentId = Guid.NewGuid();

            // Act
            user.LinkToStudent(studentId, "admin");

            // Assert
            user.StudentId.Should().Be(studentId);
        }

        [Fact]
        public void LinkToStudent_StudentUser_RaisesLinkedEvent()
        {
            // Arrange
            var user = CreateTestUser(UserType.Student);
            var studentId = Guid.NewGuid();

            // Act
            user.LinkToStudent(studentId, "admin");

            // Assert
            var linkedEvent = user.DomainEvents.OfType<UserLinkedToStudentEvent>().FirstOrDefault();
            linkedEvent.Should().NotBeNull();
            linkedEvent.StudentId.Should().Be(studentId);
        }

        [Fact]
        public void LinkToStudent_StaffUserType_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = CreateTestUser(UserType.Staff);
            var studentId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                user.LinkToStudent(studentId, "admin"));
        }

        [Fact]
        public void LinkToEmployee_StaffUserType_LinksSuccessfully()
        {
            // Arrange
            var user = CreateTestUser(UserType.Staff);
            var employeeId = Guid.NewGuid();

            // Act
            user.LinkToEmployee(employeeId, "admin");

            // Assert
            user.EmployeeId.Should().Be(employeeId);
        }

        [Fact]
        public void LinkToEmployee_StaffUser_RaisesLinkedEvent()
        {
            // Arrange
            var user = CreateTestUser(UserType.Staff);
            var employeeId = Guid.NewGuid();

            // Act
            user.LinkToEmployee(employeeId, "admin");

            // Assert
            var linkedEvent = user.DomainEvents.OfType<UserLinkedToEmployeeEvent>().FirstOrDefault();
            linkedEvent.Should().NotBeNull();
            linkedEvent.EmployeeId.Should().Be(employeeId);
        }

        [Fact]
        public void LinkToEmployee_StudentUserType_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = CreateTestUser(UserType.Student);
            var employeeId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                user.LinkToEmployee(employeeId, "admin"));
        }

        #endregion

        #region Login Management Tests

        [Fact]
        public void RecordSuccessfulLogin_UpdatesLastLoginAndResetsAttempts()
        {
            // Arrange
            var user = CreateTestUser();
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            var beforeLogin = DateTime.UtcNow;

            // Act
            user.RecordSuccessfulLogin();

            // Assert
            user.LastLoginAt.Should().NotBeNull();
            user.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
            user.LoginAttempts.Should().Be(0);
            user.LockedUntil.Should().BeNull();
        }

        [Fact]
        public void RecordSuccessfulLogin_RaisesUserLoggedInEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.RecordSuccessfulLogin();

            // Assert
            var loggedInEvent = user.DomainEvents.OfType<UserLoggedInEvent>().FirstOrDefault();
            loggedInEvent.Should().NotBeNull();
            loggedInEvent.LoginTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void RecordFailedLogin_IncrementsLoginAttempts()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));

            // Assert
            user.LoginAttempts.Should().Be(1);
        }

        [Fact]
        public void RecordFailedLogin_BelowMaxAttempts_RaisesLoginFailedEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));

            // Assert
            var failedEvent = user.DomainEvents.OfType<UserLoginFailedEvent>().FirstOrDefault();
            failedEvent.Should().NotBeNull();
            failedEvent.FailedAttempts.Should().Be(1);
        }

        [Fact]
        public void RecordFailedLogin_ReachesMaxAttempts_LocksAccount()
        {
            // Arrange
            var user = CreateTestUser();
            var lockoutDuration = TimeSpan.FromMinutes(15);

            // Act
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, lockoutDuration);
            }

            // Assert
            user.LoginAttempts.Should().Be(5);
            user.LockedUntil.Should().NotBeNull();
            user.LockedUntil.Should().BeCloseTo(
                DateTime.UtcNow.Add(lockoutDuration),
                TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void RecordFailedLogin_ReachesMaxAttempts_RaisesAccountLockedEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            // Assert
            var lockedEvent = user.DomainEvents.OfType<UserAccountLockedEvent>().FirstOrDefault();
            lockedEvent.Should().NotBeNull();
            lockedEvent.FailedAttempts.Should().Be(5);
        }

        [Fact]
        public void IsLockedOut_LockedAccount_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            // Act
            var isLocked = user.IsLockedOut();

            // Assert
            isLocked.Should().BeTrue();
        }

        [Fact]
        public void IsLockedOut_NotLockedAccount_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var isLocked = user.IsLockedOut();

            // Assert
            isLocked.Should().BeFalse();
        }

        [Fact]
        public void UnlockAccount_LockedAccount_UnlocksAndResetsAttempts()
        {
            // Arrange
            var user = CreateTestUser();
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            // Act
            user.UnlockAccount("admin");

            // Assert
            user.IsLockedOut().Should().BeFalse();
            user.LockedUntil.Should().BeNull();
            user.LoginAttempts.Should().Be(0);
        }

        [Fact]
        public void UnlockAccount_LockedAccount_RaisesAccountUnlockedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            for (int i = 0; i < 5; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            // Act
            user.UnlockAccount("admin");

            // Assert
            var unlockedEvent = user.DomainEvents.OfType<UserAccountUnlockedEvent>().FirstOrDefault();
            unlockedEvent.Should().NotBeNull();
        }

        #endregion

        #region Refresh Token Management Tests

        [Fact]
        public void AddRefreshToken_ValidToken_AddsToCollection()
        {
            // Arrange
            var user = CreateTestUser();
            var token = "refresh-token-123";
            var expiresAt = DateTime.UtcNow.AddDays(7);

            // Act
            var refreshToken = user.AddRefreshToken(token, expiresAt, "192.168.1.1");

            // Assert
            user.RefreshTokens.Should().Contain(refreshToken);
            refreshToken.Token.Should().Be(token);
        }

        [Fact]
        public void AddRefreshToken_ValidToken_RaisesTokenCreatedEvent()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var refreshToken = user.AddRefreshToken("token", DateTime.UtcNow.AddDays(7), "192.168.1.1");

            // Assert
            var tokenCreatedEvent = user.DomainEvents.OfType<RefreshTokenCreatedEvent>().FirstOrDefault();
            tokenCreatedEvent.Should().NotBeNull();
            tokenCreatedEvent.TokenId.Should().Be(refreshToken.Id);
        }

        [Fact]
        public void RevokeRefreshToken_ExistingToken_RevokesToken()
        {
            // Arrange
            var user = CreateTestUser();
            var refreshToken = user.AddRefreshToken("token", DateTime.UtcNow.AddDays(7), "192.168.1.1");

            // Act
            user.RevokeRefreshToken(refreshToken.Id, "192.168.1.1", "User logout");

            // Assert
            refreshToken.IsActive.Should().BeFalse();
            refreshToken.RevokedAt.Should().NotBeNull();
        }

        [Fact]
        public void RevokeRefreshToken_ExistingToken_RaisesTokenRevokedEvent()
        {
            // Arrange
            var user = CreateTestUser();
            var refreshToken = user.AddRefreshToken("token", DateTime.UtcNow.AddDays(7), "192.168.1.1");
            var reason = "User logout";

            // Act
            user.RevokeRefreshToken(refreshToken.Id, "192.168.1.1", reason);

            // Assert
            var revokedEvent = user.DomainEvents.OfType<RefreshTokenRevokedEvent>().FirstOrDefault();
            revokedEvent.Should().NotBeNull();
            revokedEvent.Reason.Should().Be(reason);
        }

        [Fact]
        public void RevokeRefreshToken_NonExistingToken_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = CreateTestUser();
            var nonExistingTokenId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                user.RevokeRefreshToken(nonExistingTokenId, "192.168.1.1"));
        }

        [Fact]
        public void RemoveExpiredRefreshTokens_RemovesOnlyExpiredTokens()
        {
            // Arrange
            var user = CreateTestUser();
            //user.AddRefreshToken("expired1", DateTime.UtcNow.AddDays(-1), "192.168.1.1");
            var activeToken = user.AddRefreshToken("active", DateTime.UtcNow.AddDays(7), "192.168.1.1");
            //user.AddRefreshToken("expired2", DateTime.UtcNow.AddDays(-2), "192.168.1.1");

            // Act
            user.RemoveExpiredRefreshTokens();

            // Assert
            user.RefreshTokens.Should().HaveCount(1);
            user.RefreshTokens.Should().Contain(activeToken);
        }

        [Fact]
        public void GetActiveRefreshToken_ExistingActiveToken_ReturnsToken()
        {
            // Arrange
            var user = CreateTestUser();
            var tokenValue = "active-token-123";
            user.AddRefreshToken(tokenValue, DateTime.UtcNow.AddDays(7), "192.168.1.1");

            // Act
            var token = user.GetActiveRefreshToken(tokenValue);

            // Assert
            token.Should().NotBeNull();
            token.Token.Should().Be(tokenValue);
            token.IsActive.Should().BeTrue();
        }

        [Fact]
        public void GetActiveRefreshToken_RevokedToken_ReturnsNull()
        {
            // Arrange
            var user = CreateTestUser();
            var tokenValue = "revoked-token";
            var refreshToken = user.AddRefreshToken(tokenValue, DateTime.UtcNow.AddDays(7), "192.168.1.1");
            user.RevokeRefreshToken(refreshToken.Id, "192.168.1.1");

            // Act
            var token = user.GetActiveRefreshToken(tokenValue);

            // Assert
            token.Should().BeNull();
        }

        [Fact]
        public void GetActiveRefreshToken_NonExistingToken_ReturnsNull()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var token = user.GetActiveRefreshToken("non-existing-token");

            // Assert
            token.Should().BeNull();
        }

        #endregion

        #region Helper Methods

        private User CreateTestUser(UserType userType = UserType.Student)
        {
            var email = new Email("student@school.com");
            var fullName = new FullName("John", "Doe");

            return User.Create(
                TestUsername,
                email,
                fullName,
                TestPassword,
                userType,
                CreatedBy,
                CreatedIp,
                CorrelationId);
        }

        #endregion
    }
}
