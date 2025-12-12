namespace SchoolManagement.API.Extensions
{
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                // Remove server header
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");

                // Add security headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

                // Content Security Policy
                context.Response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");

                // HSTS (production only)
                if (!context.Request.Host.Host.Contains("localhost"))
                {
                    context.Response.Headers.Add("Strict-Transport-Security",
                        "max-age=31536000; includeSubDomains; preload");
                }

                await next();
            });
        }
    }
}
