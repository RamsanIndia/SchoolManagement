using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace SchoolManagement.Persistence
{
    public class SchoolManagementDbContextFactory : IDesignTimeDbContextFactory<SchoolManagementDbContext>
    {
        public SchoolManagementDbContext CreateDbContext(string[] args)
        {
            // Set base path to the API project (where appsettings.json exists)
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
            optionsBuilder.UseSqlServer(connectionString);

            // Create a logger factory for design-time
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<SchoolManagementDbContext> logger = loggerFactory.CreateLogger<SchoolManagementDbContext>();

            // Pass null for IHttpContextAccessor at design time
            return new SchoolManagementDbContext(optionsBuilder.Options, null, logger);
        }
    }
}
