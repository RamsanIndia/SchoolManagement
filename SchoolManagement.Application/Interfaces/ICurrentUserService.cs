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
        string? UserId { get; }
        string Username { get; }
        string? Email { get; }
        string? FullName { get; }
        string? UserType { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        IEnumerable<Claim> GetAllClaims();
    }
}
