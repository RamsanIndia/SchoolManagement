using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Services;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Services;
using SchoolManagement.Infrastructure.Configuration;
using SchoolManagement.Infrastructure.EventBus;
using SchoolManagement.Infrastructure.Events;
using SchoolManagement.Infrastructure.Persistence.Repositories;
using SchoolManagement.Infrastructure.Services;
using SchoolManagement.Persistence.Repositories;
using SchoolManagement.Persistence.Services;

namespace SchoolManagement.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register all repository services
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Core Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Entity Repositories
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRoleMenuPermissionRepository, RoleMenuPermissionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<ISectionRepository, SectionRepository>();
            services.AddScoped<ISectionSubjectRepository, SectionSubjectRepository>();
            services.AddScoped<ITimeTableRepository, TimeTableRepository>();
            services.AddScoped<IBiometricDeviceRepository, BiometricDeviceRepository>();
            services.AddScoped<IOfflineAttendanceRepository, OfflineAttendanceRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();



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

            // Auth Services with Decorator Pattern
            services.AddScoped<TokenService>(); // Concrete implementation
            services.AddScoped<ITokenService, CachedTokenService>(); // Cached decorator
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IpAddressHelper>();

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
            //services.AddSingleton<IEventBus, EventBus>();

            // Register DomainEventPublisher
            
            services.AddScoped<IEventPublisher, AzureServiceBusPublisher>();
            services.AddScoped<IIntegrationEventMapper, IntegrationEventMapper>();

            return services;
        }
    }
}
