
using Azure.Messaging.ServiceBus;
using FluentValidation;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
using SchoolManagement.Infrastructure.EventBus;
using SchoolManagement.Infrastructure.Events;
using SchoolManagement.Infrastructure.Services;
using SchoolManagement.Persistence;
using SchoolManagement.Persistence.Behaviors;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// ============= STRUCTURED LOGGING (Serilog) =============
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SchoolManagement")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/school-management-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_000_000,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting School Management System API");

    // ============= DATABASE CONFIGURATION =============

    //builder.Services.AddDbContext<SchoolManagementDbContext>((serviceProvider, options) =>
    //{
    //    var connectionString = builder.Configuration.GetConnectionString("SchoolManagementDbConnectionString")
    //        ?? throw new InvalidOperationException("Database connection string not found");

    //    options.UseSqlServer(connectionString, sqlOptions =>
    //    {
    //        sqlOptions.MigrationsAssembly("SchoolManagement.API");
    //        //sqlOptions.EnableRetryOnFailure(
    //        //    maxRetryCount: 5,
    //        //    maxRetryDelay: TimeSpan.FromSeconds(30),
    //        //    errorNumbersToAdd: null
    //        //);
    //        sqlOptions.CommandTimeout(30);
    //        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    //    });

    //    // Enable sensitive data logging only in development
    //    if (builder.Environment.IsDevelopment())
    //    {
    //        options.EnableSensitiveDataLogging();
    //        options.EnableDetailedErrors();
    //    }

    //    // Disable change tracking for read-only scenarios (can be enabled per query)
    //    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    //});

    builder.Services.AddDbContext<SchoolManagementDbContext>((serviceProvider, options) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("SchoolManagementDbConnectionString")
            ?? throw new InvalidOperationException("Database connection string not found");

        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("SchoolManagement.API");
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });


    // ============= MEMORY CACHE =============
    builder.Services.AddMemoryCache();
    //builder.Services.AddMemoryCache(options =>
    //{
    //    options.SizeLimit = 1024; // Limit cache size
    //    options.CompactionPercentage = 0.25; // Compact 25% when limit reached
    //});

    // ============= REDIS CACHE (Optional but Recommended) =============
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "SchoolManagement:";
        });
        Log.Information("Redis cache configured");
    }
    else
    {
        Log.Warning("Redis not configured, using in-memory cache only");
    }

    // ============= HTTP CONTEXT ACCESSOR =============
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // ============= AUTOMAPPER =============
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AllowNullCollections = true;
        cfg.AllowNullDestinationValues = true;
    }, typeof(Program).Assembly, typeof(SchoolManagement.Application.Mappings.MappingProfile).Assembly);

    // Validate AutoMapper configuration in development
    if (builder.Environment.IsDevelopment())
    {
        var provider = builder.Services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<AutoMapper.IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        Log.Information("AutoMapper configuration validated successfully");
    }

    // ============= MEDIATR (CQRS) =============

    builder.Services.AddScoped<ITransactionManager, TransactionManager>();

    // MediatR with behaviors - ORDER MATTERS!
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(typeof(CreateStudentCommand).Assembly);

        // Execution order (outermost to innermost):
        // 1. Logging - Log the request
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

        // 2. Performance - Measure execution time
        cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));

        // 3. Validation - Validate request
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

        // 4. Concurrency Retry - MUST be before Transaction
        cfg.AddOpenBehavior(typeof(ConcurrencyRetryBehavior<,>));

        // 5. Transaction - Innermost (wraps the handler)
        cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
    });

    Log.Information("MediatR configured with pipeline behaviors");

    // ============= FLUENT VALIDATION =============
    builder.Services.AddValidatorsFromAssembly(
        typeof(SchoolManagement.Application.Validators.CreateStudentCommandValidator).Assembly
    );

    // ============= JWT AUTHENTICATION =============
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey not configured");

    if (secretKey.Length < 32)
    {
        throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");
    }

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
            ValidIssuer = jwtSettings["Issuer"] ?? "SchoolManagementSystem",

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "SchoolManagementUsers",

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,

            // ⚠️ CRITICAL FIX - Maps claims to ClaimsPrincipal Identity
            NameClaimType = ClaimTypes.Name, 
            RoleClaimType = ClaimTypes.Role  
        };

        // Enhanced JWT Bearer Events
        options.Events = new JwtBearerEvents
        {
            // For SignalR/WebSocket support
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/NotificationHub"))
                {
                    context.Token = accessToken;
                    Log.Debug("Token received from query string for SignalR: {Path}", path);
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                    Log.Warning("JWT Authentication failed: Token expired");
                }
                else
                {
                    Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                }

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var username = context.Principal?.Identity?.Name;
                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                Log.Debug("JWT Token validated successfully. User: {Username}, UserId: {UserId}",
                    username, userId);

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Log.Warning("JWT Authentication challenge: {Error}, {ErrorDescription}",
                    context.Error ?? "No error",
                    context.ErrorDescription ?? "No description");

                return Task.CompletedTask;
            }
        };
    });

    // ============= AUTHORIZATION =============
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

    // ============= DEPENDENCY INJECTION - Use Extension Methods =============
    builder.Services.AddRepositories();
    builder.Services.AddAuthenticationServices();
    builder.Services.AddApplicationServices();
    builder.Services.AddDomainServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ============= EVENT BUS CONFIGURATION =============
    var eventBusProvider = builder.Configuration["EventBus:Provider"] ?? "RabbitMQ";

    if (eventBusProvider == "AzureServiceBus")
    {
        // Get connection string
        var connectionString = builder.Configuration["AzureServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Service Bus connection string not configured");

        // Register ServiceBusClient as singleton (recommended pattern)
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

        Log.Information("Using Azure Service Bus for event publishing");
    }
    else
    {
        //builder.Services.AddSingleton<IEventBus>(sp =>
        //{
        //    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
        //    var rabbitMQHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        //    return new RabbitMQEventBus(logger, sp, rabbitMQHost);
        //});

        //Log.Information("Using RabbitMQ for event publishing");
    }

    // ============= BACKGROUND SERVICES =============
    builder.Services.AddHostedService<OutboxProcessorService>();
    builder.Services.AddHostedService<OutboxCleanupService>();
    //builder.Services.AddHostedService<AttendanceSyncService>();
    //builder.Services.AddHostedService<NotificationProcessorService>();
    //builder.Services.AddHostedService<TokenCleanupService>();

    // ============= HTTP CLIENTS =============
    builder.Services.AddHttpClient("SMS", client =>
    {
        var smsBaseUrl = builder.Configuration["NotificationSettings:SMS:BaseUrl"];
        if (!string.IsNullOrEmpty(smsBaseUrl))
        {
            client.BaseAddress = new Uri(smsBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }
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
            client.Timeout = TimeSpan.FromSeconds(30);
        }
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "SchoolManagement/1.0");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

    // ============= RATE LIMITING =============
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

        // Global rate limiter by IP
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

    // ============= CORS =============
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:8080" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });

    // ============= SWAGGER =============
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
            },
            License = new OpenApiLicense
            {
                Name = "Proprietary",
                Url = new Uri("https://schoolmanagement.com/license")
            }
        });

        // JWT Bearer Authentication
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme. 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
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

        // Custom schema filters
        c.EnableAnnotations();
        c.CustomSchemaIds(type => type.FullName);
    });

    // ============= HEALTH CHECKS =============
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<SchoolManagementDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "sql" })
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

    // Add Redis health check if configured
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        builder.Services.AddHealthChecks()
            .AddRedis(redisConnectionString, name: "redis", tags: new[] { "cache", "redis" });
    }

    // ============= CONTROLLERS =============
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // ============= COMPRESSION =============
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });

    // ============= REQUEST/RESPONSE LOGGING =============
    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        options.RequestBodyLogLimit = 4096;
        options.ResponseBodyLogLimit = 4096;
    });

    // Register ProblemDetails services + global customization
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Extensions["traceId"] =
                System.Diagnostics.Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
        };
    });

    // Register exception handler (IExceptionHandler)
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    // ============= DATABASE INITIALIZATION (Run migrations and seed data) =============
    await InitializeDatabaseAsync(app);

    // ============= MIDDLEWARE PIPELINE =============
    app.UseMiddleware<CorrelationIdMiddleware>();
    // Response compression (first for all responses)
    app.UseResponseCompression();

    // Security headers
    app.UseSecurityHeaders();

    // Exception handling
    if (app.Environment.IsDevelopment())
    {
        //app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler(); // will invoke GlobalExceptionHandler (IExceptionHandler)
        app.UseHsts();
    }

    // HTTP logging (development only)
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpLogging();
    }

    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Management System API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Hide schemas section
        c.EnableDeepLinking();
        c.EnableFilter();
    });

    // Request logging middleware
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseHttpsRedirection();

    // CORS must be before routing
    app.UseCors("AllowReactApp");

    // Rate limiting
    app.UseRateLimiter();

    app.UseRouting();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Controllers
    app.MapControllers();

    // Root endpoint
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();

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

    Log.Information("School Management System API started successfully");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// ============= HELPER METHODS =============

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}

// ============= DATABASE INITIALIZATION METHOD =============

static async Task InitializeDatabaseAsync(WebApplication app)
{
    // Only run migrations in Development or when explicitly enabled
    var runMigrations = app.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");
    var seedData = app.Configuration.GetValue<bool>("Database:SeedDataOnStartup");

    if (!runMigrations && !seedData)
    {
        app.Logger.LogInformation("Database initialization skipped (disabled in configuration)");
        return;
    }

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (runMigrations)
        {
            app.Logger.LogInformation("Running database migrations...");
            await context.Database.MigrateAsync();
        }

        if (seedData)
        {
            app.Logger.LogInformation("Seeding database...");

            // Use the new DataSeeder instead of MenuDataSeeder
            await DataSeeder.SeedAsync(context, logger);
        }

        app.Logger.LogInformation("✅ Database initialization completed");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Database initialization failed");
        throw;
    }
}

