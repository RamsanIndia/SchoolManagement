using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Domain.Exceptions;

namespace SchoolManagement.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if correlation ID exists in request header
            string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                // Generate new correlation ID if not provided
                correlationId = Guid.NewGuid().ToString();
            }

            // Store in HttpContext.Items for easy access throughout request
            context.Items["CorrelationId"] = correlationId;

            // Add to response headers
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            // Add to logging scope
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                await _next(context);
            }
        }
    }
}
