using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Test.TestFixtures
{
    public class UserBuilder
    {
        private string _username = "testuser";
        private string _email = "test@school.com";
        private string _firstName = "Test";
        private string _lastName = "User";
        private string _phoneNumber = "+1234567890";
        private string _passwordHash = "hashed-password-123";
        private UserType _userType = UserType.Student;
        private string _createdBy = "system";
        private string _createdIp = "192.168.1.1";
        private string _correlationId = "test-correlation";
        private bool _emailVerified = false;
        private bool _phoneVerified = false;
        private bool _isActive = true;
        private int _failedLoginAttempts = 0;

        public UserBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder WithName(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
            return this;
        }

        public UserBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public UserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public UserBuilder AsStudent()
        {
            _userType = UserType.Student;
            return this;
        }

        public UserBuilder AsTeacher()
        {
            _userType = UserType.Staff;
            return this;
        }

        public UserBuilder AsParent()
        {
            _userType = UserType.Parent;
            return this;
        }

        public UserBuilder AsAdmin()
        {
            _userType = UserType.Admin;
            return this;
        }

        public UserBuilder EmailVerified()
        {
            _emailVerified = true;
            return this;
        }

        public UserBuilder PhoneVerified()
        {
            _phoneVerified = true;
            return this;
        }

        public UserBuilder Inactive()
        {
            _isActive = false;
            return this;
        }

        public UserBuilder WithFailedLoginAttempts(int attempts)
        {
            _failedLoginAttempts = attempts;
            return this;
        }

        public UserBuilder Locked()
        {
            _failedLoginAttempts = 5;
            return this;
        }

        public User Build()
        {
            var email = new Email(_email);
            var fullName = new FullName(_firstName, _lastName);

            var user = User.Create(
                _username,
                email,
                fullName,
                _passwordHash,
                _userType,
                _createdBy,
                _createdIp,
                _correlationId);

            if (_emailVerified)
                user.VerifyEmail(_createdBy);

            if (_phoneVerified)
                user.VerifyPhone(_createdBy);

            if (!_isActive)
                user.Deactivate(_createdBy);

            for (int i = 0; i < _failedLoginAttempts; i++)
            {
                user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            }

            return user;
        }
    }
}
