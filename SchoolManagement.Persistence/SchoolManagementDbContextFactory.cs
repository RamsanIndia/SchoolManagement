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
            // ✅ Determine base path for configuration
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SchoolManagement.API");

            // Fallback to current directory if API project not found
            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            // ✅ Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
                    optional: true)
                .AddEnvironmentVariables()
                .Build();

            // ✅ Get connection string
            var connectionString = configuration.GetConnectionString("SchoolManagementDbConnectionString")
                ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'SchoolManagementDbConnectionString' or 'DefaultConnection' not found in configuration.");
            }

            Console.WriteLine($"Using connection string: {MaskConnectionString(connectionString)}");

            // ✅ Setup service collection for DI
            var services = new ServiceCollection();

            // Register logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
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

            // ✅ Enable detailed errors for migrations
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();

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
        public string? UserId => "00000000-0000-0000-0000-000000000000";

        public string Username => "System";

        public string? Email => "system@migration.local";

        public string? FullName => "System Migration User";

        public string? UserType => "System";

        public bool IsAuthenticated => false;

        public bool IsInRole(string role)
        {
            // During migrations, no role checking needed
            return false;
        }

        public IEnumerable<Claim> GetAllClaims()
        {
            // Return empty claims for design-time
            return new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.NameIdentifier, UserId ?? string.Empty),
                new Claim(ClaimTypes.Email, Email ?? string.Empty)
            };
        }
    }
}