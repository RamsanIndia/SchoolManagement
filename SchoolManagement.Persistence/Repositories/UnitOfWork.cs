using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ICurrentUserService _currentUserService;
        private IDbContextTransaction _transaction;

        private const int MaxRetries = 3;
        private const int BaseDelayMilliseconds = 100;

        // Repository instances
        private IAuthRepository _authRepository;
        private IRefreshTokenRepository _refreshTokenRepository;
        private IEmployeeRepository _employeeRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IRoleMenuPermissionRepository _roleMenuPermissionRepository;
        private IStudentRepository _studentRepository;
        private IAttendanceRepository _attendanceRepository;
        private IMenuRepository _menuRepository;
        private IUserRoleRepository _userRoleRepository;
        private IPermissionRepository _permissionRepository;
        private IClassRepository _classesRepository;
        private ISectionRepository _sectionsRepository;
        private ISectionSubjectRepository _sectionSubjectsRepository;
        private ITimeTableRepository _timeTablesRepository;
        private ITeacherRepository? _teachersRepository;
        private IDepartmentRepository? _departmentRepository;
        private IAcademicYearRepository? _academicYearRepository;
        private IAuditLogRepository _auditLogs;
        private ISchoolRepository _schoolRepository;

        public UnitOfWork(
            SchoolManagementDbContext context,
            ILogger<UnitOfWork> logger,
            ICurrentUserService currentUserService)  // ✅ Add this
                      // ✅ Add this
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Repository Properties
        public IAuthRepository AuthRepository =>
            _authRepository ??= new AuthRepository(_context);

        public IRefreshTokenRepository RefreshTokenRepository =>
            _refreshTokenRepository ??= new RefreshTokenRepository(_context);

        public IEmployeeRepository EmployeeRepository =>
            _employeeRepository ??= new EmployeeRepository(_context);

        public IUserRepository UserRepository =>
            _userRepository ??= new UserRepository(_context);

        public IRoleRepository RoleRepository =>
            _roleRepository ??= new RoleRepository(_context);

        public IRoleMenuPermissionRepository RoleMenuPermissionRepository =>
            _roleMenuPermissionRepository ??= new RoleMenuPermissionRepository(_context);

        public IStudentRepository StudentRepository =>
            _studentRepository ??= new StudentRepository(_context);

        public IAttendanceRepository AttendanceRepository =>
            _attendanceRepository ??= new AttendanceRepository(_context);

        public IUserRoleRepository UserRoleRepository =>
            _userRoleRepository ??= new UserRoleRepository(_context);

        public IMenuRepository MenuRepository =>
            _menuRepository ??= new MenuRepository(_context);

        public IPermissionRepository Permissions =>
            _permissionRepository ??= new PermissionRepository(_context);

        public IClassRepository ClassesRepository =>
            _classesRepository ??= new ClassRepository(_context);

        public ISectionRepository SectionsRepository =>
            _sectionsRepository ??= new SectionRepository(_context);

        public ISectionSubjectRepository SectionSubjectsRepository =>
            _sectionSubjectsRepository ??= new SectionSubjectRepository(_context);

        public ITimeTableRepository TimeTablesRepository =>
            _timeTablesRepository ??= new TimeTableRepository(_context);

        public ITeacherRepository TeachersRepository =>
            _teachersRepository ??= new TeacherRepository(_context);

        public IDepartmentRepository DepartmentRepository =>
            _departmentRepository ??= new DepartmentRepository(_context);

        public IAcademicYearRepository AcademicYearRepository =>
            _academicYearRepository ??= new AcademicYearRepository(_context);

        public IAuditLogRepository AuditLogRepository =>
            _auditLogs ??= new AuditLogRepository(_context);

        public ISchoolRepository SchoolRepository =>
            _schoolRepository ??= new SchoolRepository(_context);


        #endregion

        #region SaveChanges with Concurrency Retry Logic & Audit
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    _logger.LogDebug("💾 UnitOfWork: Attempting SaveChanges, attempt {Attempt}", retryCount + 1);

                    // ✅ APPLY AUDIT INFO BEFORE SAVING
                    ApplyAuditInfo();

                    // Log what's being tracked
                    LogTrackedEntities();

                    var result = await _context.SaveChangesAsync(cancellationToken);

                    if (retryCount > 0)
                    {
                        _logger.LogInformation("✅ UnitOfWork: SaveChanges succeeded after {Retries} retries", retryCount);
                    }
                    else
                    {
                        _logger.LogDebug("✅ UnitOfWork: SaveChanges succeeded. {Count} entities saved.", result);
                    }

                    return result;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;

                    var affectedEntities = string.Join(", ",
                        ex.Entries.Select(e => $"{e.Entity.GetType().Name}"));

                    _logger.LogWarning(ex,
                        "⚠️ UnitOfWork: Concurrency conflict on {Entities}. Attempt {Attempt}/{Max}",
                        affectedEntities, retryCount, MaxRetries);

                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogError(ex,
                            "❌ UnitOfWork: Concurrency conflict after {Retries} retries. Aborting.",
                            retryCount);

                        throw new InvalidOperationException(
                            "Unable to save changes due to concurrent modifications. Please reload and try again.",
                            ex);
                    }

                    var hasUnresolvableConflicts = false;

                    foreach (var entry in ex.Entries)
                    {
                        var entityType = entry.Entity.GetType().Name;
                        var state = entry.State;

                        _logger.LogDebug("🔄 Resolving concurrency for {EntityType} (State: {State})",
                            entityType, state);

                        try
                        {
                            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                            if (databaseValues == null)
                            {
                                _logger.LogWarning(
                                    "⚠️ Entity {EntityType} was deleted in DB. Detaching.",
                                    entityType);
                                entry.State = EntityState.Detached;
                                continue;
                            }

                            switch (state)
                            {
                                case EntityState.Modified:
                                    entry.OriginalValues.SetValues(databaseValues);
                                    _logger.LogDebug(
                                        "✅ Merged changes for {EntityType} (Client Wins strategy)",
                                        entityType);
                                    break;

                                case EntityState.Deleted:
                                    _logger.LogWarning(
                                        "⚠️ Attempted delete on {EntityType} already deleted. Detaching.",
                                        entityType);
                                    entry.State = EntityState.Detached;
                                    break;

                                case EntityState.Added:
                                    var existsInDb = databaseValues != null;
                                    if (existsInDb)
                                    {
                                        _logger.LogWarning(
                                            "⚠️ Attempted add on {EntityType} that now exists. Converting to Modified.",
                                            entityType);
                                        entry.State = EntityState.Modified;
                                        entry.OriginalValues.SetValues(databaseValues);
                                    }
                                    break;
                            }
                        }
                        catch (Exception resolveEx)
                        {
                            _logger.LogError(resolveEx,
                                "❌ Error resolving concurrency for entity {EntityType}", entityType);
                            hasUnresolvableConflicts = true;
                        }
                    }

                    if (hasUnresolvableConflicts)
                    {
                        _logger.LogError(
                            "❌ Unresolvable conflicts detected. Aborting after attempt {Attempt}",
                            retryCount);
                        throw;
                    }

                    var delay = BaseDelayMilliseconds * (int)Math.Pow(2, retryCount - 1);
                    _logger.LogDebug("⏳ Waiting {Delay}ms before retry {Retry}", delay, retryCount + 1);
                    await Task.Delay(delay, cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex,
                        "❌ UnitOfWork: DbUpdateException - {Message}",
                        ex.InnerException?.Message ?? ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ UnitOfWork: Unexpected error during SaveChanges");
                    throw;
                }
            }
        }

        // ✅ NEW METHOD: Apply audit information to tracked entities
        private void ApplyAuditInfo()
        {
            var entries = _context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            if (!entries.Any())
            {
                _logger.LogDebug("📝 No entities require audit info");
                return;
            }

            // Get current user info from services
            var currentUser = _currentUserService.Username;
            var currentIp = _currentUserService.IpAddress;
            var isAuthenticated = _currentUserService.IsAuthenticated;

            _logger.LogInformation(
                "📝 Applying audit info to {Count} entities - User: {User}, IP: {IP}, IsAuth: {IsAuth}",
                entries.Count,
                currentUser,
                currentIp,
                isAuthenticated
            );

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId = entry.Entity.Id;

                if (entry.State == EntityState.Added)
                {
                    _logger.LogDebug(
                        "   ➕ Setting Created audit for {EntityType} ({Id}): CreatedBy={User}",
                        entityType,
                        entityId,
                        currentUser
                    );
                    entry.Entity.SetCreated(currentUser, currentIp);
                }

                if (entry.State == EntityState.Modified)
                {
                    _logger.LogDebug(
                        "   ✏️ Setting Updated audit for {EntityType} ({Id}): UpdatedBy={User}",
                        entityType,
                        entityId,
                        currentUser
                    );
                    entry.Entity.SetUpdated(currentUser, currentIp);
                }
            }
        }

        // Helper method to log tracked entities
        private void LogTrackedEntities()
        {
            var entries = _context.ChangeTracker.Entries().ToList();

            if (!entries.Any())
            {
                _logger.LogDebug("📊 ChangeTracker: No tracked entities");
                return;
            }

            _logger.LogDebug("📊 ChangeTracker has {Count} tracked entities:", entries.Count);

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;
                var id = entry.Entity is BaseEntity be ? be.Id.ToString() : "N/A";

                _logger.LogDebug("   - {EntityType} (ID: {Id}): {State}", entityType, id, entry.State);

                // Special logging for entities with audit info
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        _logger.LogDebug(
                            "      📝 Audit: CreatedBy={CreatedBy}, CreatedAt={CreatedAt}, CreatedIp={CreatedIp}",
                            baseEntity.CreatedBy,
                            baseEntity.CreatedAt,
                            baseEntity.CreatedIP
                        );
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        _logger.LogDebug(
                            "      📝 Audit: UpdatedBy={UpdatedBy}, UpdatedAt={UpdatedAt}, UpdatedIp={UpdatedIp}",
                            baseEntity.UpdatedBy,
                            baseEntity.UpdatedAt,
                            baseEntity.CreatedIP
                        );
                    }
                }

                // Special logging for RefreshToken
                if (entry.Entity is RefreshToken rt && entry.State == EntityState.Added)
                {
                    _logger.LogDebug(
                        "      🔑 RefreshToken: UserId={UserId}, Token={Token}..., Expiry={Expiry}",
                        rt.UserId,
                        rt.Token?.Substring(0, Math.Min(10, rt.Token?.Length ?? 0)),
                        rt.ExpiryDate
                    );
                }
            }
        }
        #endregion

        #region Transaction Handling
        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (_transaction != null)
            {
                _logger.LogDebug("⚠️ Transaction already active, skipping BeginTransaction");
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogDebug("🔵 Transaction started");
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                    _logger.LogDebug("✅ Transaction committed");
                }
                else
                {
                    _logger.LogWarning("⚠️ CommitTransaction called but no active transaction");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error committing transaction");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync(cancellationToken);
                    _logger.LogDebug("🔙 Transaction rolled back");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error rolling back transaction");
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                    _context.ChangeTracker.Clear();
                    _logger.LogDebug("🧹 ChangeTracker cleared after rollback");
                }
            }
            else
            {
                _logger.LogWarning("⚠️ RollbackTransaction called but no active transaction");
            }
        }

        public async Task ExecuteInTransactionAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                _logger.LogDebug("⚠️ Already in transaction, executing without new transaction");
                await action();
                return;
            }

            await BeginTransactionAsync(cancellationToken);
            try
            {
                await action();
                await SaveChangesAsync(cancellationToken);
                await CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during transaction execution, rolling back");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> func,
            CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                _logger.LogDebug("⚠️ Already in transaction, executing without new transaction");
                return await func();
            }

            await BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await func();
                await SaveChangesAsync(cancellationToken);
                await CommitTransactionAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during transaction execution, rolling back");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        #endregion

        #region Disposal
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
                    _logger.LogDebug("🗑️ UnitOfWork disposed");
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }

                if (_context != null)
                {
                    await _context.DisposeAsync();
                }

                _logger.LogDebug("🗑️ UnitOfWork disposed (async)");
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _context.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        }
        #endregion
    }
}