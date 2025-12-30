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
    public class AuthResponseBuilder : IAuthResponseBuilder
    {

        public AuthResponseDto BuildAuthResponse(User user, string accessToken, string refreshToken) => new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email.Value,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                PhoneNumber = user.PhoneNumber?.Value,
                IsEmailVerified = user.EmailVerified,
                IsPhoneVerified = user.PhoneVerified,
                LastLoginAt = (DateTime)user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                Roles = new List<string> { user.UserType.ToString() }
            }
        };
    }
}
