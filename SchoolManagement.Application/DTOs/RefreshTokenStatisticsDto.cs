using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class RefreshTokenStatisticsDto
    {
        public int TotalTokens { get; set; }
        public int ActiveTokens { get; set; }
        public int RevokedTokens { get; set; }
        public int ExpiredTokens { get; set; }
        public DateTime? OldestActiveToken { get; set; }
        public DateTime? NewestActiveToken { get; set; }
        public int ActiveSessionsCount => ActiveTokens;

        /// <summary>
        /// Indicates if user has too many active sessions
        /// </summary>
        public bool HasTooManySessions(int maxSessions = 5) => ActiveTokens > maxSessions;
    }
}
