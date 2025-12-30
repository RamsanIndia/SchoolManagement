using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    /// <summary>
    /// Handles account security checks
    /// </summary>
    public interface IAccountSecurityService
    {
        Task<Result> ValidateAccountStatusAsync(User user);
        Task<Result> VerifyCredentialsAsync(User user, string password);
        Task HandleFailedLoginAsync(User user, CancellationToken cancellationToken);
    }
}
