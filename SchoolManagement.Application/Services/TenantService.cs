using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid TenantId =>
            GetValue<Guid>("TenantId");

        public Guid? SchoolId =>
            GetOptionalValue<Guid>("SchoolId");

        public string? SchoolName =>
            GetOptionalValue<string>("SchoolName");

        public bool IsTenantSet =>
            TenantId != Guid.Empty;

        public bool IsSchoolSet =>
            SchoolId.HasValue;

        private T GetValue<T>(string key)
        {
            var ctx = _httpContextAccessor.HttpContext
                      ?? throw new InvalidOperationException("No HttpContext");

            if (!ctx.Items.TryGetValue(key, out var value))
                throw new InvalidOperationException($"{key} not set");

            return (T)value;
        }

        private T? GetOptionalValue<T>(string key)
        {
            var ctx = _httpContextAccessor.HttpContext;
            return ctx != null && ctx.Items.TryGetValue(key, out var value)
                ? (T?)value
                : default;
        }
    }

}
