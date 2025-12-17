using Microsoft.AspNetCore.Http;
using System.Linq;

namespace SchoolManagement.Application.Shared.Utilities
{
    /// <summary>
    /// Utility class for extracting client IP addresses from HTTP requests
    /// </summary>
    public class IpAddressHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpAddressHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get current user ID from HTTP context
        /// </summary>
        public string GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value
                   ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("userId")?.Value
                   ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                   ?? "System";
        }

        /// <summary>
        /// Gets the client IP address from the current HTTP request.
        /// Checks X-Forwarded-For header first (for proxy/load balancer scenarios),
        /// then falls back to direct connection IP.
        /// </summary>
        /// <returns>Client IP address or "Unknown" if unavailable</returns>
        public string GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null)
                return Constants.UnknownIpAddress;

            // Check for forwarded headers (proxy/load balancer)
            var forwardedFor = httpContext.Request.Headers[Constants.ForwardedForHeader].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
                return forwardedFor.Split(',')[0].Trim();

            // Get direct connection IP
            return httpContext.Connection.RemoteIpAddress?.ToString()
                ?? Constants.UnknownIpAddress;
        }

        /// <summary>
        /// Gets the correlation ID from request headers or generates a new one
        /// </summary>
        /// <returns>Correlation ID for request tracking</returns>
        public string GetCorrelationId()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null)
                return System.Guid.NewGuid().ToString();

            return httpContext.Request.Headers[Constants.CorrelationIdHeader].FirstOrDefault()
                ?? System.Guid.NewGuid().ToString();
        }
    }
}
