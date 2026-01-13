// Persistence/SchoolManagementDbContextFactory.cs - MULTI-TENANT READY
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Persistence.Interceptors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SchoolManagement.Persistence
{
    /// <summary>
    /// Design-time factory for EF migrations (multi-tenant aware)
    /// </summary>
    public class SchoolManagementDbContextFactory : IDesignTimeDbContextFactory<SchoolManagementDbContext>
    {
        public SchoolManagementDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var isProd = env.Equals("Production", StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"🔧 {env} mode - Multi-tenant migrations");

            // 🔧 BASE PATH (API project)
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SchoolManagement.API");
            if (!Directory.Exists(basePath)) basePath = Directory.GetCurrentDirectory();

            // 🔧 CONFIG (env-aware)
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // 🔧 CONNECTION STRING (secure fallback)
            var connStr = isProd
                ? Environment.GetEnvironmentVariable("ConnectionStrings__SchoolManagementDbConnectionString")
                : config.GetConnectionString("SchoolManagementDbConnectionString")
                ?? config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("❌ Connection string missing");

            Console.WriteLine($"✅ Conn: {MaskConnectionString(connStr)}");

            // 🔧 SERVICES (design-time)
            var services = new ServiceCollection();
            services.AddLogging(b => b.AddConsole().SetMinimumLevel(isProd ? LogLevel.Warning : LogLevel.Information));
            services.AddSingleton<IHttpContextAccessor, DesignTimeHttpContextAccessor>();

            // ✅ MULTI-TENANT MOCKS
            services.AddSingleton<ICurrentUserService, DesignTimeCurrentUserService>();
            services.AddSingleton<ICorrelationIdService, DesignTimeCorrelationIdService>();
            services.AddSingleton<ITenantService, DesignTimeTenantService>();
            services.AddSingleton<IpAddressHelper>();
            services.AddSingleton<AuditInterceptor>();

            var sp = services.BuildServiceProvider();

            // 🔧 EF OPTIONS (PostgreSQL optimized)
            var options = new DbContextOptionsBuilder<SchoolManagementDbContext>();
            options.UseNpgsql(connStr, pg =>
            {
                pg.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                pg.CommandTimeout(60);
                pg.MigrationsAssembly("SchoolManagement.Persistence");
            });

            if (!isProd)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }

            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());

            // 🔧 RESOLVE DEPENDENCIES
            return new SchoolManagementDbContext(
                options.Options,
                sp.GetRequiredService<IHttpContextAccessor>(),
                sp.GetRequiredService<ILogger<SchoolManagementDbContext>>(),
                sp.GetRequiredService<IpAddressHelper>(),
                sp.GetRequiredService<ICurrentUserService>(),
                sp.GetRequiredService<ICorrelationIdService>(),
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<ITenantService>());
        }

        private static string MaskConnectionString(string connStr) =>
            Regex.Replace(connStr, @"(Password|Pwd)=([^;]+)", "$1=****", RegexOptions.IgnoreCase);
    }

    #region DESIGN-TIME MOCKS (Multi-Tenant)

    /// <summary>
    /// HttpContext for design-time (migrations)
    /// </summary>
    internal class DesignTimeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();
    }

    /// <summary>
    /// System user for migrations/seeding
    /// </summary>
    internal class DesignTimeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => false;
        public Guid? UserId => Guid.Empty;
        public string Username => "MigrationSystem";
        public string? Email => "migration@system.local";
        public string? FirstName => "System";
        public string? LastName => "Migration";
        public string? FullName => "System Migration";
        public string? UserType => "System";
        public string IpAddress => "127.0.0.1";
        public string UserAgent => "EF Core Migrations";

        public string? GetClaim(string claimType) =>
            claimType switch
            {
                ClaimTypes.NameIdentifier => UserId.ToString(),
                ClaimTypes.Name => Username,
                ClaimTypes.Email => Email,
                ClaimTypes.Role => UserType,
                _ => null
            };

        public IEnumerable<string> GetRoles() => new[] { "System", "Migration" };
        public bool IsInRole(string role) => role == "System";
        public bool HasPermission(string permission) => true;

        public IEnumerable<Claim> GetAllClaims() => new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
            new Claim(ClaimTypes.Name, Username),
            new Claim(ClaimTypes.Role, "System")
        };

        public string GetRequestPath() => "/ef-migrations";
        public string GetRequestMethod() => "MIGRATE";
    }

    /// <summary>
    /// Correlation ID generator (design-time)
    /// </summary>
    internal class DesignTimeCorrelationIdService : ICorrelationIdService
    {
        public string GetCorrelationId() => $"migration-{Guid.NewGuid():N}[0..16]";
    }

    /// <summary>
    /// ✅ FIXED: Multi-tenant mock for migrations
    /// System tenant + default school for seeding
    /// </summary>
    internal class DesignTimeTenantService : ITenantService
    {
        private static readonly Guid SystemTenantId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        private static readonly Guid DefaultSchoolId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public Guid TenantId => SystemTenantId;
        public Guid? SchoolId => DefaultSchoolId;
        public string? SchoolName => "Migration Default School";
        public string? TenantCode => "MIGRATION";
        public bool IsTenantSet => true;
        public bool IsSchoolSet => true;

        // Explicit interface impl for legacy
        Guid SchoolManagement.Application.Interfaces.ITenantService.TenantId => TenantId;
        bool SchoolManagement.Application.Interfaces.ITenantService.IsSchoolSet => IsSchoolSet;
    }

    #endregion
}