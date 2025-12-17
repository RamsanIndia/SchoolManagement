using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Shared.Utilities;
using System;
using System.IO;

namespace SchoolManagement.Persistence
{
    public class SchoolManagementDbContextFactory : IDesignTimeDbContextFactory<SchoolManagementDbContext>
    {
        public SchoolManagementDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SchoolManagement.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
                    optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("SchoolManagementDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'SchoolManagementDbConnectionString' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<SchoolManagementDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<SchoolManagementDbContext> logger = loggerFactory.CreateLogger<SchoolManagementDbContext>();

            // Design-time: no real HTTP request exists, but you can still provide an accessor instance.
            var httpContextAccessor = new HttpContextAccessor();

            // Create whatever your helper needs (adjust constructor to your real one)
            var ipAddressHelper = new IpAddressHelper(httpContextAccessor);

            return new SchoolManagementDbContext(
                optionsBuilder.Options,
                httpContextAccessor,
                logger,
                ipAddressHelper);
        }
    }
}
