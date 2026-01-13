using Azure.Messaging.ServiceBus;
using FluentValidation;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using SchoolManagement.API.Authorization;
using SchoolManagement.API.Extensions;
using SchoolManagement.API.Middleware;
using SchoolManagement.Application.Behaviors;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Services;
using SchoolManagement.Application.Students.Commands;
using SchoolManagement.Infrastructure.BackgroundServices;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Events;
using SchoolManagement.Infrastructure.Services;
using SchoolManagement.Infrastructure.Swagger;
using SchoolManagement.Persistence;
using SchoolManagement.Persistence.Behaviors;
using SchoolManagement.Persistence.Middleware;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

// ============================================================================
// GLOBAL EXCEPTION HANDLER FOR STARTUP FAILURES
// ============================================================================
try
{
    var builder = WebApplication.CreateBuilder(args);

    // ========================================================================
    // STRUCTURED LOGGING (Serilog)
    // ========================================================================
    ConfigureLogging(builder);

    Log.Information("========================================");
    Log.Information("Starting School Management System API");
    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("========================================");

    // ========================================================================
    // CONFIGURATION VALIDATION (Fail Fast)
    // ========================================================================
    ValidateConfiguration(builder.Configuration, builder.Environment);

    // ========================================================================
    // DATABASE CONFIGURATION
    // ========================================================================
    ConfigureDatabase(builder);

    // ========================================================================
    // CACHING
    // ========================================================================
    ConfigureCaching(builder);

    // ========================================================================
    // APPLICATION SERVICES
    // ========================================================================
    ConfigureApplicationServices(builder);

    // ========================================================================
    // AUTHENTICATION & AUTHORIZATION
    // ========================================================================
    ConfigureAuthentication(builder);
    ConfigureAuthorization(builder);

    // ========================================================================
    // DEPENDENCY INJECTION
    // ========================================================================
    ConfigureDependencyInjection(builder);

    // ========================================================================
    // EVENT BUS (Optional)
    // ========================================================================
    ConfigureEventBus(builder);

    // ========================================================================
    // BACKGROUND SERVICES
    // ========================================================================
    ConfigureBackgroundServices(builder);

    // ========================================================================
    // HTTP CLIENTS WITH RESILIENCE
    // ========================================================================
    ConfigureHttpClients(builder);

    // ========================================================================
    // RATE LIMITING
    // ========================================================================
    ConfigureRateLimiting(builder);

    // ========================================================================
    // CORS
    // ========================================================================
    ConfigureCors(builder);

    // ========================================================================
    // SWAGGER
    // ========================================================================
    ConfigureSwagger(builder);

    // ========================================================================
    // HEALTH CHECKS (Production-Ready)
    // ========================================================================
    ConfigureHealthChecks(builder);

    // ========================================================================
    // CONTROLLERS & API BEHAVIOR
    // ========================================================================
    ConfigureControllers(builder);

    // ========================================================================
    // RESPONSE COMPRESSION
    // ========================================================================
    ConfigureCompression(builder);

    // ========================================================================
    // PROBLEM DETAILS & EXCEPTION HANDLING
    // ========================================================================
    ConfigureExceptionHandling(builder);

    // ========================================================================
    // BUILD APPLICATION
    // ========================================================================
    var app = builder.Build();

    Log.Information("Application built successfully, configuring middleware pipeline...");

    // ========================================================================
    // AUTOMAPPER VALIDATION (After Build)
    // ========================================================================
    ValidateAutoMapperConfiguration(app);

    // ========================================================================
    // MIDDLEWARE PIPELINE (Order Matters!)
    // ========================================================================
    ConfigureMiddleware(app);

    // ========================================================================
    // ENDPOINT MAPPING
    // ========================================================================
    ConfigureEndpoints(app);

    // ========================================================================
    // DATABASE INITIALIZATION (Non-Blocking Background Task)
    // ========================================================================
    ScheduleDatabaseInitialization(app);

    Log.Information("========================================");
    Log.Information("✅ School Management System API Ready");
    Log.Information("========================================");

    // ========================================================================
    // START APPLICATION
    // ========================================================================
    await app.RunAsync();
}
catch (HostAbortedException)
{
    // This can happen during EF Core migrations/tooling - can be safely ignored
    Log.Information("Host was aborted (likely EF Core tooling)");
}
catch (Exception ex)
{
    // Log to both console and file for critical startup failures
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    var errorMessage = $"[{timestamp}] FATAL: Application start-up failed: {ex.Message}";

    Console.Error.WriteLine(errorMessage);
    Console.Error.WriteLine(ex.StackTrace);

    if (Log.Logger != null)
    {
        Log.Fatal(ex, "Application start-up failed");

        // Log inner exceptions for better debugging
        var innerEx = ex.InnerException;
        var depth = 1;
        while (innerEx != null && depth <= 5)
        {
            Log.Fatal(innerEx, "Inner Exception [{Depth}]: {Message}", depth, innerEx.Message);
            innerEx = innerEx.InnerException;
            depth++;
        }

        Log.CloseAndFlush();
    }

    // Exit with error code
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}

// ============================================================================
// CONFIGURATION METHODS
// ============================================================================

static void ConfigureLogging(WebApplicationBuilder builder)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "SchoolManagement")
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        .Enrich.WithMachineName()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: builder.Environment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
        .WriteTo.File(
            path: "logs/school-management-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 10_000_000,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();
    Log.Information("✅ Serilog configured");
}

static void ValidateConfiguration(IConfiguration configuration, IHostEnvironment environment)
{
    Log.Information("Validating configuration...");

    var errors = new List<string>();

    // Required: Database Connection String
    var dbConnectionString = configuration.GetConnectionString("SchoolManagementDbConnectionString");
    if (string.IsNullOrWhiteSpace(dbConnectionString))
    {
        errors.Add("Database connection string 'SchoolManagementDbConnectionString' is missing");
    }

    // Required: JWT Settings
    var jwtSecretKey = configuration["Jwt:SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtSecretKey))
    {
        errors.Add("JWT SecretKey is missing (Jwt:SecretKey)");
    }
    else if (jwtSecretKey.Length < 32)
    {
        errors.Add($"JWT SecretKey must be at least 32 characters (current: {jwtSecretKey.Length})");
    }

    if (string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]))
    {
        errors.Add("JWT Issuer is missing (Jwt:Issuer)");
    }

    if (string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]))
    {
        errors.Add("JWT Audience is missing (Jwt:Audience)");
    }

    // Optional: Event Bus (only validate if provider is set)
    var eventBusProvider = configuration["EventBus:Provider"];
    if (!string.IsNullOrEmpty(eventBusProvider) && eventBusProvider == "AzureServiceBus")
    {
        var serviceBusConnection = configuration["AzureServiceBus:ConnectionString"];
        if (string.IsNullOrWhiteSpace(serviceBusConnection))
        {
            Log.Warning("Azure Service Bus provider selected but connection string is missing");
        }
    }

    if (errors.Any())
    {
        Log.Fatal("Configuration validation failed:");
        foreach (var error in errors)
        {
            Log.Fatal("  ❌ {Error}", error);
        }
        throw new InvalidOperationException(
            $"Configuration validation failed. Missing required settings: {string.Join(", ", errors)}");
    }

    Log.Information("✅ Configuration validated successfully");
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<SchoolManagementDbContext>((serviceProvider, options) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("SchoolManagementDbConnectionString")!;

        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("SchoolManagement.API");
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });

        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });

    Log.Information("✅ Database context configured with PostgreSQL");
}

static void ConfigureCaching(WebApplicationBuilder builder)
{
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache();

    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        try
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "SchoolManagement:";
            });
            Log.Information("✅ Redis cache configured");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis configuration failed, falling back to in-memory cache");
        }
    }
    else
    {
        Log.Information("Redis not configured, using in-memory cache only");
    }
}

static void ConfigureApplicationServices(WebApplicationBuilder builder)
{
    // HTTP Context Accessor
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // AutoMapper
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AllowNullCollections = true;
        cfg.AllowNullDestinationValues = true;
    }, typeof(Program).Assembly, typeof(SchoolManagement.Application.Mappings.MappingProfile).Assembly);

    // ❌ REMOVED: BuildServiceProvider() call - this was causing HostAbortedException
    // AutoMapper validation moved to after app.Build() - see ValidateAutoMapperConfiguration()

    // Transaction Manager
    builder.Services.AddScoped<ITransactionManager, TransactionManager>();

    // MediatR with behaviors (Order matters!)
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(typeof(CreateStudentCommand).Assembly);

        // Pipeline behaviors (outermost to innermost)
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));           // 1. Logging
        cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));       // 2. Performance monitoring
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));        // 3. Validation
        cfg.AddOpenBehavior(typeof(ConcurrencyRetryBehavior<,>));  // 4. Concurrency retry
        cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));       // 5. Transaction (innermost)
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(
        typeof(SchoolManagement.Application.Validators.CreateStudentCommandValidator).Assembly);

    Log.Information("✅ Application services configured");
}

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"]!;

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/NotificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                    Log.Warning("JWT token expired for {Path}", context.Request.Path);
                }
                else
                {
                    Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                }
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var username = context.Principal?.Identity?.Name;
                Log.Debug("JWT token validated for user: {Username}", username);
                return Task.CompletedTask;
            }
        };
    });

    Log.Information("✅ JWT authentication configured");
}

static void ConfigureAuthorization(WebApplicationBuilder builder)
{
    builder.Services.AddAuthorization(options =>
    {
        // Role-based policies
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
        options.AddPolicy("StudentOrParent", policy => policy.RequireRole("Student", "Parent"));
        options.AddPolicy("StaffAccess", policy => policy.RequireRole("Admin", "Principal", "Teacher"));
        options.AddPolicy("ParentAccess", policy => policy.RequireRole("Parent"));

        // Permission-based policies
        options.AddPolicy("StudentManagement", policy =>
            policy.Requirements.Add(new MenuPermissionAttribute("StudentManagement", "view")));
        options.AddPolicy("StudentAdd", policy =>
            policy.Requirements.Add(new MenuPermissionAttribute("StudentManagement", "add")));
        options.AddPolicy("EmployeeManagement", policy =>
            policy.Requirements.Add(new MenuPermissionAttribute("HRMSManagement", "view")));
        options.AddPolicy("AttendanceView", policy =>
            policy.Requirements.Add(new MenuPermissionAttribute("AttendanceManagement", "view")));
        options.AddPolicy("SystemAdmin", policy =>
            policy.Requirements.Add(new MenuPermissionAttribute("SystemSettings", "view")));

        // Fallback policy - require authenticated users by default
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    builder.Services.AddScoped<IAuthorizationHandler, MenuPermissionHandler>();
    Log.Information("✅ Authorization policies configured");
}

static void ConfigureDependencyInjection(WebApplicationBuilder builder)
{
    builder.Services.AddRepositories();
    builder.Services.AddAuthenticationServices();
    builder.Services.AddApplicationServices();
    builder.Services.AddDomainServices();
    builder.Services.AddTimeTableServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    Log.Information("✅ Dependency injection configured");
}

static void ConfigureEventBus(WebApplicationBuilder builder)
{
    var eventBusProvider = builder.Configuration["EventBus:Provider"];

    if (string.IsNullOrEmpty(eventBusProvider))
    {
        Log.Information("Event bus not configured (optional)");
        return;
    }

    if (eventBusProvider == "AzureServiceBus")
    {
        var connectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.Services.AddSingleton<ServiceBusClient>(sp =>
            {
                var clientOptions = new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpTcp,
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        Mode = ServiceBusRetryMode.Exponential,
                        MaxRetries = 3,
                        Delay = TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(30)
                    }
                };

                return new ServiceBusClient(connectionString, clientOptions);
            });

            Log.Information("✅ Azure Service Bus configured");
        }
        else
        {
            Log.Warning("Azure Service Bus provider selected but connection string is missing");
        }
    }
}

static void ConfigureBackgroundServices(WebApplicationBuilder builder)
{
    builder.Services.AddHostedService<OutboxProcessorService>();
    builder.Services.AddHostedService<OutboxCleanupService>();

    Log.Information("✅ Background services registered");
}

static void ConfigureHttpClients(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient("SMS", client =>
    {
        var smsBaseUrl = builder.Configuration["NotificationSettings:SMS:BaseUrl"];
        if (!string.IsNullOrEmpty(smsBaseUrl))
        {
            client.BaseAddress = new Uri(smsBaseUrl);
        }
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "SchoolManagement/1.0");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

    builder.Services.AddHttpClient("Email", client =>
    {
        var emailBaseUrl = builder.Configuration["NotificationSettings:Email:BaseUrl"];
        if (!string.IsNullOrEmpty(emailBaseUrl))
        {
            client.BaseAddress = new Uri(emailBaseUrl);
        }
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "SchoolManagement/1.0");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

    Log.Information("✅ HTTP clients configured with resilience policies");
}

static void ConfigureRateLimiting(WebApplicationBuilder builder)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Auth endpoints - stricter limits
        options.AddFixedWindowLimiter("AuthPolicy", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 10;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });

        // General API endpoints
        options.AddFixedWindowLimiter("GeneralPolicy", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 100;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 10;
        });

        // File upload endpoints
        options.AddFixedWindowLimiter("UploadPolicy", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(5);
            opt.PermitLimit = 20;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 3;
        });

        // Global rate limiter by IP/User
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var userIdentifier = context.User.Identity?.Name
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userIdentifier,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
        });

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                await context.HttpContext.Response.WriteAsync(
                    $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.",
                    cancellationToken);
            }
            else
            {
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.",
                    cancellationToken);
            }

            Log.Warning("Rate limit exceeded for {User} on {Path}",
                context.HttpContext.User.Identity?.Name ?? "Anonymous",
                context.HttpContext.Request.Path);
        };
    });

    Log.Information("✅ Rate limiting configured");
}

static void ConfigureCors(WebApplicationBuilder builder)
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:8080" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    Log.Information("✅ CORS configured for origins: {Origins}", string.Join(", ", allowedOrigins));
}

static void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "School Management System API",
            Version = "v1",
            Description = "Enterprise-grade School Management System with Clean Architecture, CQRS, and Event-Driven Architecture",
            Contact = new OpenApiContact
            {
                Name = "Development Team",
                Email = "dev@schoolmanagement.com",
                Url = new Uri("https://schoolmanagement.com")
            }
        });

        // JWT Bearer Authentication
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // XML Comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        c.EnableAnnotations();
        c.CustomSchemaIds(type => type.FullName);
        //c.OperationFilter<AddRequiredHeadersFilter>();
    });

    Log.Information("✅ Swagger configured");
}

static void ConfigureHealthChecks(WebApplicationBuilder builder)
{
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        // Self check - always healthy (for liveness)
        .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "live" })

        // Database check (for readiness) - degraded instead of unhealthy
        .AddDbContextCheck<SchoolManagementDbContext>(
            name: "database",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ready", "db" });

    // Redis health check (optional)
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        healthChecksBuilder.AddRedis(
            redisConnectionString,
            name: "redis",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ready", "cache" });
    }

    Log.Information("✅ Health checks configured");
}

static void ConfigureControllers(WebApplicationBuilder builder)
{
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    Log.Information("✅ Controllers configured");
}

static void ConfigureCompression(WebApplicationBuilder builder)
{
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });
}

static void ConfigureExceptionHandling(WebApplicationBuilder builder)
{
    // HTTP Logging (Development only)
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            options.RequestBodyLogLimit = 4096;
            options.ResponseBodyLogLimit = 4096;
        });
    }

    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Extensions["traceId"] =
                System.Diagnostics.Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Extensions["instance"] = ctx.HttpContext.Request.Path;
        };
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    Log.Information("✅ Exception handling configured");
}

static void ValidateAutoMapperConfiguration(WebApplication app)
{
    // ✅ AutoMapper validation moved here - AFTER app.Build()
    // This avoids creating a duplicate service provider during startup
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            Log.Information("✅ AutoMapper configuration validated");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ AutoMapper configuration validation failed");
            throw;
        }
    }
}

static void ConfigureMiddleware(WebApplication app)
{
    // 1. Correlation ID (first – tracing)
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. Response compression
    app.UseResponseCompression();

    // 3. Security headers
    app.UseSecurityHeaders();

    // 4. Global exception handling
    if (app.Environment.IsDevelopment())
    {
        // Developer exception page (optional)
    }
    app.UseExceptionHandler();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // 5. HTTP logging (dev only)
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpLogging();
    }

    // 6. Swagger (OK before auth)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Management System API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
        c.EnableDeepLinking();
        c.EnableFilter();
    });

    // 7. Request logging
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 8. HTTPS redirection
    app.UseHttpsRedirection();

    // 9. CORS (before routing)
    app.UseCors("AllowReactApp");

    // 10. Rate limiting (before routing)
    app.UseRateLimiter();

    // 11. Routing
    app.UseRouting();

    

    // 13. Authentication (now tenant-aware)
    app.UseAuthentication();

    // 14. Authorization
    app.UseAuthorization();

    // 🔐 12. TENANT RESOLUTION (CRITICAL POSITION)
    app.UseMiddleware<TenantMiddleware>();

    Log.Information("✅ Middleware pipeline configured (Tenant-aware)");
}

static void ConfigureEndpoints(WebApplication app)
{
    // Health checks - production-ready
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    }).AllowAnonymous();

    // Kubernetes readiness probe
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    }).AllowAnonymous();

    // Kubernetes liveness probe
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    // Startup probe (returns healthy immediately after startup)
    app.MapHealthChecks("/health/startup", new HealthCheckOptions
    {
        Predicate = _ => false, // No checks, just returns healthy
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    // Controllers
    app.MapControllers();

    // Root endpoint
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription()
        .AllowAnonymous();

    // API info endpoint
    app.MapGet("/api/info", () => new
    {
        Application = "School Management System",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow
    })
    .AllowAnonymous()
    .WithTags("System");

    Log.Information("✅ Endpoints mapped");
}

static void ScheduleDatabaseInitialization(WebApplication app)
{
    var runMigrations = app.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");
    var seedData = app.Configuration.GetValue<bool>("Database:SeedDataOnStartup");

    if (!runMigrations && !seedData)
    {
        Log.Information("Database initialization disabled in configuration");
        return;
    }

    // Run database initialization in background (non-blocking)
    _ = Task.Run(async () =>
    {
        // Wait a few seconds to let the app start
        await Task.Delay(TimeSpan.FromSeconds(5));

        try
        {
            Log.Information("Starting database initialization (background task)...");

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            if (runMigrations)
            {
                Log.Information("Running database migrations...");
                await context.Database.MigrateAsync();
                Log.Information("✅ Database migrations completed");
            }

            if (seedData)
            {
                Log.Information("Seeding database...");
                await DataSeeder.SeedAsync(context, logger);
                Log.Information("✅ Database seeding completed");
            }

            Log.Information("✅ Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Database initialization failed (non-critical, app will continue)");
        }
    });

    Log.Information("Database initialization scheduled as background task");
}

// ============================================================================
// HELPER METHODS
// ============================================================================

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Retry {RetryCount} after {Delay}ms due to {Error}",
                    retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? "HTTP error");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Log.Error("Circuit breaker opened for {Duration}s due to {Error}",
                    duration.TotalSeconds, outcome.Exception?.Message ?? "HTTP error");
            },
            onReset: () =>
            {
                Log.Information("Circuit breaker reset");
            });
}