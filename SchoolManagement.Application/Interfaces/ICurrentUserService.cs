using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ICurrentUserService
    {
        // Core identity properties
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string Username { get; }
        string? Email { get; }

        // User details
        string? FirstName { get; }
        string? LastName { get; }
        string? FullName { get; }
        string? UserType { get; }

        // Request context (for audit logging)
        string IpAddress { get; }
        string UserAgent { get; }

        // Additional methods
        string? GetClaim(string claimType);
        IEnumerable<string> GetRoles();
        bool IsInRole(string role);
        bool HasPermission(string permission);
        IEnumerable<Claim> GetAllClaims();
        string GetRequestPath();
        string GetRequestMethod();
    }
}
