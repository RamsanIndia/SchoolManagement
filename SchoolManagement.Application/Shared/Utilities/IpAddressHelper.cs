using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SchoolManagement.Application.Shared.Utilities
{
    /// <summary>
    /// Utility class for extracting client IP addresses and user context from HTTP requests
    /// </summary>
    public class IpAddressHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpAddressHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the client's IP address, considering proxies and load balancers
        /// </summary>
        public string GetIpAddress()
        {
            var context = _httpContextAccessor?.HttpContext;
            if (context == null)
                return "Unknown";

            // Check X-Forwarded-For header first (standard for proxies/load balancers)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For can contain multiple IPs (client, proxy1, proxy2, ...)
                // First IP is the original client
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            // Check X-Real-IP header (alternative used by some proxies like nginx)
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp.Trim();
            }

            // Fallback to RemoteIpAddress
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp))
            {
                // Handle IPv6 loopback
                if (remoteIp == "::1")
                    return "127.0.0.1";

                return remoteIp;
            }

            return "Unknown";
        }

        /// <summary>
        /// Get current user ID from HTTP context with proper claim type checking
        /// </summary>
        //public string GetCurrentUserId()
        //{
        //    var user = _httpContextAccessor?.HttpContext?.User;
        //    if (user == null || !user.Identity?.IsAuthenticated == true)
        //        return "System";

        //    // Try ClaimTypes.NameIdentifier first (standard ASP.NET Core)
        //    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!string.IsNullOrEmpty(userId))
        //        return userId;

        //    // Try JWT standard 'sub' claim
        //    userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        //    if (!string.IsNullOrEmpty(userId))
        //        return userId;

        //    // Try custom 'userId' claim
        //    userId = user.FindFirst("userId")?.Value;
        //    if (!string.IsNullOrEmpty(userId))
        //        return userId;

        //    // Fallback to Identity.Name
        //    return user.Identity?.Name ?? "System";
        //}

        ///// <summary>
        ///// Get current username from HTTP context
        ///// </summary>
        //public string GetCurrentUsername()
        //{
        //    var user = _httpContextAccessor?.HttpContext?.User;
        //    if (user == null || !user.Identity?.IsAuthenticated == true)
        //        return "System";

        //    // Try ClaimTypes.Name first
        //    var username = user.FindFirst(ClaimTypes.Name)?.Value;
        //    if (!string.IsNullOrEmpty(username))
        //        return username;

        //    // Try custom 'username' claim
        //    username = user.FindFirst("username")?.Value;
        //    if (!string.IsNullOrEmpty(username))
        //        return username;

        //    // Fallback to Identity.Name
        //    return user.Identity?.Name ?? "System";
        //}

        ///// <summary>
        ///// Gets user agent from request headers
        ///// </summary>
        //public string GetUserAgent()
        //{
        //    var context = _httpContextAccessor?.HttpContext;
        //    if (context == null)
        //        return "Unknown";

        //    return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        //}

        ///// <summary>
        ///// Gets request path for audit logging
        ///// </summary>
        //public string GetRequestPath()
        //{
        //    var context = _httpContextAccessor?.HttpContext;
        //    if (context == null)
        //        return "Unknown";

        //    return $"{context.Request.Method} {context.Request.Path}";
        //}
    }
}