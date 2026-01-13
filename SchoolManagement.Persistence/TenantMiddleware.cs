using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolManagement.Persistence;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            try
            {
                // 1. Skip for Swagger/static endpoints
                var path = context.Request.Path.Value?.ToLower() ?? "";
                if (path.Contains("/swagger") || path.Contains("/health") || path == "/")
                {
                    await _next(context);
                    return;
                }

                // 2. Extract tenant/school codes from multiple sources
                var tenantCode = ExtractTenantCode(context);
                var schoolCode = ExtractSchoolCode(context);

                // 3. For Login endpoint: Parse body JSON
                if (path == "/api/auth/login" && context.Request.ContentType?.Contains("application/json") == true)
                {
                    (tenantCode, schoolCode) = await ParseLoginBodyAsync(context, tenantCode, schoolCode);
                }
                // 4. For authenticated requests: Extract from JWT token
                else if (context.User?.Identity?.IsAuthenticated == true)
                {
                    (tenantCode, schoolCode) = ExtractFromJwtToken(context, tenantCode, schoolCode);
                }

                // 5. Only use defaults for unauthenticated requests (like initial login without tenant)
                // For authenticated requests, tenant MUST be present
                if (string.IsNullOrWhiteSpace(tenantCode))
                {
                    // Check if this is an authenticated request
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        throw new UnauthorizedAccessException("Tenant information missing from authenticated request");
                    }

                    // For login endpoint, allow default for backward compatibility
                    if (path == "/api/auth/login")
                    {
                        tenantCode = "GLOBAL";
                        _logger.LogWarning("⚠️ Using default tenantCode for login: GLOBAL");
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Tenant code is required");
                    }
                }

                if (string.IsNullOrWhiteSpace(schoolCode) && path == "/api/auth/login")
                {
                    schoolCode = "SCH001";
                    _logger.LogWarning("⚠️ Using default schoolCode for login: SCH001");
                }

                _logger.LogInformation("🔍 TenantMiddleware: Tenant={TenantCode}, School={SchoolCode}, Path={Path}, Authenticated={IsAuth}",
                    tenantCode, schoolCode, path, context.User?.Identity?.IsAuthenticated);

                // 6. Resolve tenant/school from database
                await ResolveTenantAndSchoolAsync(context, serviceProvider, tenantCode, schoolCode);

                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "❌ Unauthorized: {Message}", ex.Message);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 TenantMiddleware fatal error");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant resolution failed" });
            }
        }

        private string? ExtractTenantCode(HttpContext context)
        {
            return context.Request.Headers["X-Tenant-Code"].FirstOrDefault()
                ?? context.Request.Query["tenantCode"].FirstOrDefault();
        }

        private string? ExtractSchoolCode(HttpContext context)
        {
            return context.Request.Headers["X-School-Code"].FirstOrDefault()
                ?? context.Request.Query["schoolCode"].FirstOrDefault();
        }

        // NEW: Extract tenant/school from JWT claims
        private (string? tenantCode, string? schoolCode) ExtractFromJwtToken(HttpContext context, string? tenantCode, string? schoolCode)
        {
            try
            {
                // If already provided via headers/query, use those
                if (!string.IsNullOrWhiteSpace(tenantCode) && !string.IsNullOrWhiteSpace(schoolCode))
                {
                    _logger.LogInformation("✅ Using header/query values - TenantCode: {TenantCode}, SchoolCode: {SchoolCode}",
                        tenantCode, schoolCode);
                    return (tenantCode, schoolCode);
                }

                var user = context.User;

                // ✅ CRITICAL DEBUG: Log if user is authenticated and all claims
                _logger.LogInformation("🔐 User authenticated: {IsAuth}, Claims count: {ClaimCount}",
                    user?.Identity?.IsAuthenticated,
                    user?.Claims?.Count() ?? 0);

                if (user?.Claims != null && user.Claims.Any())
                {
                    _logger.LogInformation("🎫 All JWT Claims: [{Claims}]",
                        string.Join(" | ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
                }
                else
                {
                    _logger.LogWarning("⚠️ No claims found in user principal!");
                    return (tenantCode, schoolCode);
                }

                // Extract from claims
                tenantCode ??= user.FindFirst("TenantCode")?.Value
                            ?? user.FindFirst("tenantCode")?.Value
                            ?? user.FindFirst(ClaimTypes.GroupSid)?.Value;

                schoolCode ??= user.FindFirst("SchoolCode")?.Value
                            ?? user.FindFirst("schoolCode")?.Value;

                if (!string.IsNullOrWhiteSpace(tenantCode))
                {
                    _logger.LogInformation("✅ Extracted from JWT: TenantCode={TenantCode}, SchoolCode={SchoolCode}",
                        tenantCode, schoolCode ?? "NULL");
                }
                else
                {
                    _logger.LogError("❌ TenantCode NOT FOUND in JWT claims! Available claim types: [{ClaimTypes}]",
                        string.Join(", ", user.Claims.Select(c => c.Type).Distinct()));
                }

                return (tenantCode, schoolCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Failed to extract tenant from JWT token");
                return (tenantCode, schoolCode);
            }
        }

        private async Task<(string? tenantCode, string? schoolCode)> ParseLoginBodyAsync(
            HttpContext context, string? tenantCode, string? schoolCode)
        {
            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("tenantCode", out var tc))
                        tenantCode ??= tc.GetString();
                    if (doc.RootElement.TryGetProperty("schoolCode", out var sc))
                        schoolCode ??= sc.GetString();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse login body JSON");
            }

            return (tenantCode, schoolCode);
        }

        private async Task ResolveTenantAndSchoolAsync(
            HttpContext context,
            IServiceProvider serviceProvider,
            string tenantCode,
            string schoolCode)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();

            // ✅ Resolve Tenant
            _logger.LogDebug("🔍 Searching tenant: {TenantCode}", tenantCode);

            var tenant = await dbContext.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Code == tenantCode && t.IsActive && !t.IsDeleted);

            if (tenant == null)
            {
                var availableTenants = await dbContext.Tenants
                    .IgnoreQueryFilters()
                    .Where(t => t.IsActive)
                    .Select(t => t.Code)
                    .ToListAsync();

                _logger.LogError("❌ Tenant NOT FOUND: {TenantCode}. Available: [{Tenants}]",
                    tenantCode, string.Join(", ", availableTenants));

                throw new UnauthorizedAccessException($"Invalid tenant code: {tenantCode}");
            }

            context.Items["TenantId"] = tenant.Id;
            context.Items["TenantCode"] = tenant.Code; // Add this for reference
            _logger.LogInformation("✅ Tenant resolved: {TenantCode} → {TenantId}", tenantCode, tenant.Id);

            // ✅ Resolve School
            if (!string.IsNullOrWhiteSpace(schoolCode))
            {
                _logger.LogDebug("🔍 Searching school: {SchoolCode} in tenant {TenantId}", schoolCode, tenant.Id);

                var school = await dbContext.Schools
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.Code == schoolCode &&
                        s.TenantId == tenant.Id &&
                        s.IsActive &&
                        !s.IsDeleted);

                if (school == null)
                {
                    var availableSchools = await dbContext.Schools
                        .IgnoreQueryFilters()
                        .Where(s => s.TenantId == tenant.Id && s.IsActive)
                        .Select(s => new { s.Code, s.Name })
                        .ToListAsync();

                    _logger.LogError("❌ School NOT FOUND: {SchoolCode} in tenant {TenantId}. Available: [{Schools}]",
                        schoolCode, tenant.Id, string.Join(", ", availableSchools.Select(s => s.Code)));

                    throw new UnauthorizedAccessException($"Invalid school code: {schoolCode}");
                }

                context.Items["SchoolId"] = school.Id;
                context.Items["SchoolCode"] = school.Code; // Add this for reference
                context.Items["SchoolName"] = school.Name;
                _logger.LogInformation("✅ School resolved: {SchoolCode} → {SchoolId} ({SchoolName})",
                    schoolCode, school.Id, school.Name);
            }
            else
            {
                context.Items["SchoolId"] = Guid.Empty;
                _logger.LogDebug("No school specified - global tenant context");
            }
        }
    }
}
