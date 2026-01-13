using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; } // Seconds until access token expires
        public string TokenType { get; set; } = "Bearer";

        public string TenantCode { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }

        public UserDto User { get; set; }
    }
}
