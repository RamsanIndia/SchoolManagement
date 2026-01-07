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
        private HttpContext? Context => _httpContextAccessor.HttpContext;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                if (!IsAuthenticated) return null;

                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User?.FindFirst("nameid")?.Value
                    ?? User?.FindFirst("userId")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("UserId not found in claims");
                    return null;
                }

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                _logger.LogWarning("UserId claim '{UserIdClaim}' is not a valid GUID", userIdClaim);
                return null;
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

                var username = User?.Identity?.Name
                    ?? User?.FindFirst(ClaimTypes.Name)?.Value
                    ?? User?.FindFirst("unique_name")?.Value
                    ?? User?.FindFirst("username")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning(
                        "Username not found in claims. Available claims: {Claims}",
                        string.Join(", ", User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>())
                    );
                    return "Unknown";
                }

                return username;
            }
        }

        public string? Email
        {
            get
            {
                if (!IsAuthenticated) return null;

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

                return User?.FindFirst(ClaimTypes.GivenName)?.Value
                    ?? User?.FindFirst("given_name")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value;
            }
        }

        public string? LastName
        {
            get
            {
                if (!IsAuthenticated) return null;

                return User?.FindFirst(ClaimTypes.Surname)?.Value
                    ?? User?.FindFirst("family_name")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;
            }
        }

        public string? FullName
        {
            get
            {
                if (!IsAuthenticated) return null;

                // Try custom fullName claim first
                var fullName = User?.FindFirst("fullName")?.Value;

                if (!string.IsNullOrEmpty(fullName))
                {
                    return fullName;
                }

                // Construct from first and last name
                var firstName = FirstName;
                var lastName = LastName;

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    return $"{firstName} {lastName}";
                }

                return firstName ?? lastName ?? Username;
            }
        }

        public string? UserType
        {
            get
            {
                if (!IsAuthenticated) return null;

                return User?.FindFirst(ClaimTypes.Role)?.Value
                    ?? User?.FindFirst("role")?.Value
                    ?? User?.FindFirst(JwtRegisteredClaimNames.Typ)?.Value;
            }
        }

        public string IpAddress
        {
            get
            {
                if (Context == null) return "Unknown";

                // Check for forwarded IP (behind proxy/load balancer)
                var forwardedFor = Context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
                }

                // Check for real IP header
                var realIp = Context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Fall back to remote IP address
                var remoteIpAddress = Context.Connection?.RemoteIpAddress?.ToString();

                // Handle IPv6 loopback
                if (remoteIpAddress == "::1")
                {
                    return "127.0.0.1";
                }

                return remoteIpAddress ?? "Unknown";
            }
        }

        public string UserAgent
        {
            get
            {
                if (Context == null) return "Unknown";

                var userAgent = Context.Request.Headers["User-Agent"].FirstOrDefault();

                if (string.IsNullOrEmpty(userAgent))
                {
                    return "Unknown";
                }

                // Truncate if too long (safety measure)
                const int maxLength = 500;
                return userAgent.Length > maxLength
                    ? userAgent.Substring(0, maxLength)
                    : userAgent;
            }
        }

        public string? GetClaim(string claimType)
        {
            if (!IsAuthenticated) return null;

            return User?.FindFirst(claimType)?.Value;
        }

        public IEnumerable<string> GetRoles()
        {
            if (!IsAuthenticated) return Enumerable.Empty<string>();

            return User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                ?? Enumerable.Empty<string>();
        }

        public bool IsInRole(string role)
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(role))
                return false;

            return User?.IsInRole(role) ?? false;
        }

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(permission))
                return false;

            return User?.HasClaim("permission", permission) ?? false;
        }

        public IEnumerable<Claim> GetAllClaims()
        {
            return User?.Claims ?? Enumerable.Empty<Claim>();
        }

        public string GetRequestPath()
        {
            return Context?.Request?.Path.Value ?? "Unknown";
        }

        public string GetRequestMethod()
        {
            return Context?.Request?.Method ?? "Unknown";
        }
    }
}
