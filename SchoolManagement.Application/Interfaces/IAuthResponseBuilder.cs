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
    /// Builds authentication responses
    /// </summary>
    public interface IAuthResponseBuilder
    {
        AuthResponseDto BuildAuthResponse(User user, string accessToken, string refreshToken);
    }
}
