using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Infrastructure.Persistence.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
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
        public IClassRepository _classesRepository;
        public ISectionRepository _sectionsRepository;
        public ISectionSubjectRepository _sectionSubjectsRepository;
        public ITimeTableRepository _timeTablesRepository;

        public UnitOfWork(SchoolManagementDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Repository Properties (shared DbContext)
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
        #endregion

        #region SaveChanges with Concurrency Retry Logic
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    _logger.LogDebug("💾 UnitOfWork: Attempting SaveChanges, attempt {Attempt}", retryCount + 1);
                    var result = await _context.SaveChangesAsync(cancellationToken);

                    if (retryCount > 0)
                    {
                        _logger.LogInformation("✅ UnitOfWork: SaveChanges succeeded after {Retries} retries", retryCount);
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

                    // CRITICAL: Proper concurrency resolution
                    var hasUnresolvableConflicts = false;

                    foreach (var entry in ex.Entries)
                    {
                        var entityType = entry.Entity.GetType().Name;
                        var state = entry.State;

                        _logger.LogDebug("🔄 Resolving concurrency for {EntityType} (State: {State})",
                            entityType, state);

                        try
                        {
                            // Get current values from database
                            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                            if (databaseValues == null)
                            {
                                // Entity was deleted in DB
                                _logger.LogWarning(
                                    "⚠️ Entity {EntityType} was deleted in DB. Detaching.",
                                    entityType);
                                entry.State = EntityState.Detached;
                                continue;
                            }

                            switch (state)
                            {
                                case EntityState.Modified:
                                    // STRATEGY: Client Wins (with merge)
                                    // For login scenario, we want to keep our changes but acknowledge DB state

                                    // Log what changed in DB vs our changes
                                    var currentValues = entry.CurrentValues;
                                    var dbPropertyNames = databaseValues.Properties
                                        .Where(p => !Equals(currentValues[p.Name], databaseValues[p.Name]))
                                        .Select(p => p.Name)
                                        .ToList();

                                    if (dbPropertyNames.Any())
                                    {
                                        _logger.LogDebug(
                                            "🔍 DB changed properties for {EntityType}: {Properties}",
                                            entityType,
                                            string.Join(", ", dbPropertyNames));
                                    }

                                    // Set original values to current DB values
                                    // This makes EF think we're updating from the latest DB state
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
                                    // For added entities, check if they now exist
                                    var existsInDb = databaseValues != null;
                                    if (existsInDb)
                                    {
                                        _logger.LogWarning(
                                            "⚠️ Attempted add on {EntityType} that now exists. Converting to Modified.",
                                            entityType);

                                        // Convert to Modified state
                                        entry.State = EntityState.Modified;
                                        entry.OriginalValues.SetValues(databaseValues);
                                    }
                                    else
                                    {
                                        _logger.LogDebug(
                                            "⏩ Added entity {EntityType} will be retried.",
                                            entityType);
                                    }
                                    break;

                                default:
                                    _logger.LogDebug(
                                        "⏩ Entity {EntityType} in state {State}, no special handling.",
                                        entityType, state);
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

                    // Exponential backoff before retry
                    var delay = BaseDelayMilliseconds * (int)Math.Pow(2, retryCount - 1);
                    _logger.LogDebug("⏳ Waiting {Delay}ms before retry {Retry}", delay, retryCount + 1);
                    await Task.Delay(delay, cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    // Let the DbContext handle duplicate key errors and other DB exceptions
                    _logger.LogWarning(ex,
                        "⚠️ UnitOfWork: DbUpdateException caught. Propagating to allow DbContext error handling.");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ UnitOfWork: Unexpected error during SaveChanges");
                    throw;
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

                    // Clear change tracker after rollback to prevent stale state
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
        #endregion
    }
}