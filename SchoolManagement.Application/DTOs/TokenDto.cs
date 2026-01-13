using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } = 0;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;

        public string TenantCode { get; set; }
        public string SchoolCode { get; set; }
    }
}
