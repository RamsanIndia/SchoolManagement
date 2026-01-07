using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;

namespace SchoolManagement.Persistence
{
    /// <summary>
    /// Design-time factory for EF Core migrations and database operations
    /// </summary>
    public class SchoolManagementDbContextFactory : IDesignTimeDbContextFactory<SchoolManagementDbContext>
    {
        public SchoolManagementDbContext CreateDbContext(string[] args)
        {
            // ✅ Determine environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"🔧 Running in {environment} environment");

            // ✅ Determine base path for configuration
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SchoolManagement.API");

            // Fallback to current directory if API project not found
            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            // ✅ Build configuration (Environment variables take precedence)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables() // This automatically prioritizes env vars
                .Build();

            // ✅ Get connection string (automatically checks env vars first, then appsettings)
            var connectionString = isProduction
                ? Environment.GetEnvironmentVariable("ConnectionStrings__SchoolManagementDbConnectionString")
                  ?? configuration.GetConnectionString("SchoolManagementDbConnectionString")
                  ?? configuration.GetConnectionString("DefaultConnection")
                : configuration.GetConnectionString("SchoolManagementDbConnectionString")
                  ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var errorMessage = isProduction
                    ? "❌ Production: Connection string must be set as environment variable 'ConnectionStrings__SchoolManagementDbConnectionString'"
                    : "❌ Connection string 'SchoolManagementDbConnectionString' or 'DefaultConnection' not found in configuration.";

                throw new InvalidOperationException(errorMessage);
            }

            Console.WriteLine($"✅ Using connection string from: {(isProduction && Environment.GetEnvironmentVariable("ConnectionStrings__SchoolManagementDbConnectionString") != null ? "Environment Variable" : "appsettings.json")}");
            Console.WriteLine($"   Masked: {MaskConnectionString(connectionString)}");

            // ✅ Setup service collection for DI
            var services = new ServiceCollection();

            // Register logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(isProduction ? LogLevel.Warning : LogLevel.Information);
            });

            // Register HttpContextAccessor (needed for IpAddressHelper)
            services.AddHttpContextAccessor();

            // ✅ Register design-time stub implementations
            services.AddSingleton<ICorrelationIdService, DesignTimeCorrelationIdService>();
            services.AddSingleton<ICurrentUserService, DesignTimeCurrentUserService>();
            services.AddSingleton<IpAddressHelper>();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // ✅ Configure DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<SchoolManagementDbContext>();

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // ✅ PostgreSQL retry configuration
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.MigrationsAssembly("SchoolManagement.Persistence");
            });

            // ✅ Enable detailed errors only in non-production
            if (!isProduction)
            {
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging();
            }

            // ✅ Resolve services from DI container
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var logger = serviceProvider.GetRequiredService<ILogger<SchoolManagementDbContext>>();
            var ipAddressHelper = serviceProvider.GetRequiredService<IpAddressHelper>();
            var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();
            var correlationIdService = serviceProvider.GetRequiredService<ICorrelationIdService>();

            Console.WriteLine("✅ Design-time DbContext created successfully");

            return new SchoolManagementDbContext(
                optionsBuilder.Options,
                httpContextAccessor,
                logger,
                ipAddressHelper,
                currentUserService,
                correlationIdService);
        }

        /// <summary>
        /// Mask sensitive information in connection string for logging
        /// </summary>
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;

            // Mask password in connection string
            var maskedString = System.Text.RegularExpressions.Regex.Replace(
                connectionString,
                @"Password=([^;]+)",
                "Password=****",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return maskedString;
        }
    }

    /// <summary>
    /// Design-time stub for ICorrelationIdService (used during migrations)
    /// Returns a constant correlation ID for migration operations
    /// </summary>
    internal class DesignTimeCorrelationIdService : ICorrelationIdService
    {
        public string GetCorrelationId()
        {
            return "migration-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }

    /// <summary>
    /// Design-time stub for ICurrentUserService (used during migrations)
    /// Returns system user information for migration operations
    /// </summary>
    internal class DesignTimeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => false;

        public Guid? UserId => Guid.Empty; // 00000000-0000-0000-0000-000000000000

        public string Username => "System";

        public string? Email => "system@migration.local";

        public string? FirstName => "System";

        public string? LastName => "Migration";

        public string? FullName => "System Migration User";

        public string? UserType => "System";

        public string IpAddress => "127.0.0.1";

        public string UserAgent => "EF Core Migration";

        public string? GetClaim(string claimType)
        {
            return claimType switch
            {
                ClaimTypes.NameIdentifier => UserId.ToString(),
                ClaimTypes.Name => Username,
                ClaimTypes.Email => Email,
                ClaimTypes.GivenName => FirstName,
                ClaimTypes.Surname => LastName,
                ClaimTypes.Role => UserType,
                _ => null
            };
        }

        public IEnumerable<string> GetRoles()
        {
            return new[] { "System" };
        }

        public bool IsInRole(string role)
        {
            return role?.Equals("System", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public bool HasPermission(string permission)
        {
            // System user has all permissions during migrations
            return true;
        }

        public IEnumerable<Claim> GetAllClaims()
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimTypes.GivenName, FirstName),
                new Claim(ClaimTypes.Surname, LastName),
                new Claim(ClaimTypes.Role, UserType),
                new Claim("fullName", FullName)
            };
        }

        public string GetRequestPath()
        {
            return "/migrations";
        }

        public string GetRequestMethod()
        {
            return "MIGRATION";
        }
    }
}
