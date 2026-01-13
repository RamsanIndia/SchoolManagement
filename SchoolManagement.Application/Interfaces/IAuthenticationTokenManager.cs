using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    /// <summary>
    /// Manages authentication tokens
    /// </summary>
    public interface IAuthenticationTokenManager
    {
        //Task<TokenDto> GenerateTokensAsync(User user, string ipAddress, CancellationToken cancellationToken);
        Task<TokenDto> GenerateTokensAsync(User user, string clientIp, Guid tenantId, Guid schoolId, CancellationToken cancellationToken);

    }
}
