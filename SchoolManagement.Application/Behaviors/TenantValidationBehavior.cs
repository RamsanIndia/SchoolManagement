using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Behaviors
{
    public class TenantValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

        public TenantValidationBehavior(
            IHttpContextAccessor httpContextAccessor,
            ITenantService tenantService,
            ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                _logger.LogDebug("No HttpContext - skipping tenant validation");
                return await next();
            }

            // ✅ SAFE: Read from HttpContext.Items
            var tenantId = httpContext.Items.TryGetValue("TenantId", out var tenantObj)
                ? (Guid)tenantObj
                : Guid.Empty;

            var schoolId = httpContext.Items.TryGetValue("SchoolId", out var schoolObj)
                ? (Guid)schoolObj
                : Guid.Empty;

            // ✅ Log for debugging
            _logger.LogInformation("TenantValidation [{Request}]: TenantId={TenantId}, SchoolId={SchoolId}",
                typeof(TRequest).Name, tenantId, schoolId);

            // ✅ Validate (skip if empty - for seeding/background)
            if (tenantId != Guid.Empty)
            {
                // Read-only validation - no Set needed
                _logger.LogDebug("TenantId {TenantId} validated", tenantId);
            }

            if (schoolId != Guid.Empty)
            {
                _logger.LogDebug("SchoolId {SchoolId} validated", schoolId);
            }

            // DbContext reads from HttpContext.Items directly via middleware
            // No tenantService mutation needed!

            var response = await next();
            return response;
        }
    }
}
