using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public string? UserId
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Your token uses "nameid" which maps to ClaimTypes.NameIdentifier
                return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User?.FindFirst("userId")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }
        }

        public string Username
        {
            get
            {
                if (!IsAuthenticated)
                {
                    return "System";
                }

                // With NameClaimType = ClaimTypes.Name configured,
                // Identity.Name should now work and map to "unique_name"
                var username = User?.Identity?.Name;

                if (!string.IsNullOrEmpty(username))
                {
                    return username;
                }

                // Fallback - try all possible claim types
                username = User?.FindFirst(ClaimTypes.Name)?.Value       // Standard claim
                    ?? User?.FindFirst("unique_name")?.Value            // JWT serialization
                    ?? User?.FindFirst("username")?.Value               // Custom claim
                    ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning(
                        "Username not found in any claim. Available claims: {Claims}",
                        string.Join(", ", User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>())
                    );
                    return "System";
                }

                return username;
            }
        }

        public string? Email
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Your token uses "email" claim
                return User?.FindFirst(ClaimTypes.Email)?.Value
                    ?? User?.FindFirst("email")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            }
        }

        public string? FirstName
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Your token uses "given_name" which maps to ClaimTypes.GivenName
                return User?.FindFirst(ClaimTypes.GivenName)?.Value
                    ?? User?.FindFirst("given_name")?.Value;
            }
        }

        public string? LastName
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Your token uses "family_name" which maps to ClaimTypes.Surname
                return User?.FindFirst(ClaimTypes.Surname)?.Value
                    ?? User?.FindFirst("family_name")?.Value;
            }
        }

        public string? FullName
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Your token has custom "fullName" claim
                var fullName = User?.FindFirst("fullName")?.Value;

                if (!string.IsNullOrEmpty(fullName))
                {
                    return fullName;
                }

                // Fallback: construct from first and last name
                var firstName = FirstName;
                var lastName = LastName;

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    return $"{firstName} {lastName}";
                }

                return firstName ?? lastName;
            }
        }

        public string? UserType
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Your token uses "role" which maps to ClaimTypes.Role
                return User?.FindFirst(ClaimTypes.Role)?.Value
                    ?? User?.FindFirst("role")?.Value;
            }
        }

        public bool IsInRole(string role)
        {
            if (!IsAuthenticated) return false;

            return User?.IsInRole(role) ?? false;
        }

        public IEnumerable<Claim> GetAllClaims()
        {
            return User?.Claims ?? Enumerable.Empty<Claim>();
        }
    }
}
