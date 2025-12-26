using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Services;
using SchoolManagement.Application.Shared.Correlation;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Services;
using SchoolManagement.Infrastructure.BackgroundServices;
using SchoolManagement.Infrastructure.Configuration;
using SchoolManagement.Infrastructure.EventBus;
using SchoolManagement.Infrastructure.Events;
using SchoolManagement.Infrastructure.Persistence.Repositories;
using SchoolManagement.Infrastructure.Services;
using SchoolManagement.Persistence;
using SchoolManagement.Persistence.Repositories;
using SchoolManagement.Persistence.Services;

namespace SchoolManagement.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register repositories to share the same DbContext instance
        /// This ensures proper change tracking across all repositories
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //  Register all repositories using factory pattern to share DbContext
            // This ensures IUserRepository and IUnitOfWork.UserRepository use the SAME context

            services.AddScoped<IUserRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new UserRepository(context);
            });

            services.AddScoped<IMenuRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new MenuRepository(context);
            });

            services.AddScoped<IRoleRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new RoleRepository(context);
            });

            services.AddScoped<IPermissionRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new PermissionRepository(context);
            });

            services.AddScoped<IRoleMenuPermissionRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new RoleMenuPermissionRepository(context);
            });

            services.AddScoped<IStudentRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new StudentRepository(context);
            });

            services.AddScoped<IEmployeeRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new EmployeeRepository(context);
            });

            services.AddScoped<IAttendanceRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new AttendanceRepository(context);
            });

            services.AddScoped<IAuthRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new AuthRepository(context);
            });

            services.AddScoped<IClassRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new ClassRepository(context);
            });

            services.AddScoped<ISectionRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new SectionRepository(context);
            });

            services.AddScoped<ISectionSubjectRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new SectionSubjectRepository(context);
            });

            services.AddScoped<ITimeTableRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new TimeTableRepository(context);
            });

            services.AddScoped<IBiometricDeviceRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new BiometricDeviceRepository(context);
            });

            services.AddScoped<IOfflineAttendanceRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new OfflineAttendanceRepository(context);
            });

            services.AddScoped<IRefreshTokenRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new RefreshTokenRepository(context);
            });

            services.AddScoped<IUserRoleRepository>(sp =>
            {
                var context = sp.GetRequiredService<SchoolManagementDbContext>();
                return new UserRoleRepository(context);
            });

            return services;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountSecurityService, AccountSecurityService>();
            services.AddScoped<IAuthenticationTokenManager, AuthenticationTokenManager>();
            services.AddScoped<IAuthResponseBuilder, AuthResponseBuilder>();
            services.AddScoped<TokenService>(); // Concrete implementation
            services.AddScoped<ITokenService, CachedTokenService>(); // Cached decorator
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IpAddressHelper>();
            services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

            return services;
        }

        /// <summary>
        /// Register all application services
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Core Application Services
            services.AddScoped<IMenuPermissionService, MenuPermissionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IChangeTrackerService, ChangeTrackerService>();

            
            return services;
        }

        /// <summary>
        /// Register all domain services
        /// </summary>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAttendanceCalculationService, AttendanceCalculationService>();
            services.AddScoped<ISalaryCalculationService, SalaryCalculationService>();

            return services;
        }

        /// <summary>
        /// Register all infrastructure services
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // External Integration Services
            services.AddScoped<IBiometricVerificationService, BiometricVerificationService>();

            // Configuration Options
            services.Configure<BiometricSettings>(configuration.GetSection("BiometricSettings"));
            services.Configure<NotificationSettings>(configuration.GetSection("NotificationSettings"));
            services.Configure<AttendanceSettings>(configuration.GetSection("AttendanceSettings"));

            // Register EventBus
            services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

            // Register DomainEventPublisher
            //services.AddScoped<IEventPublisher, AzureServiceBusPublisher>();
            //services.AddScoped<IIntegrationEventMapper, IntegrationEventMapper>();
            services.AddSingleton<IEventRouter, EventRouter>();
            services.AddHostedService<NotificationEventConsumer>();

            return services;
        }
    }
}
