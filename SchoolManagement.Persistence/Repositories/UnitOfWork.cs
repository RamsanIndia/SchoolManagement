using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SchoolManagementDbContext _context;
        private IDbContextTransaction _transaction;

        // Repository instances
        private IAuthRepository _authRepository;
        private IEmployeeRepository _employeeRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IRoleMenuPermissionRepository _roleMenuPermissionRepository;
        private IStudentRepository _studentRepository;
        private IAttendanceRepository _attendanceRepository;
        private IMenuRepository _menuRepository;
        // Add other repositories as needed
        private IUserRoleRepository _userRoleRepository;
        private IPermissionRepository _permissionRepository;

        public IClassRepository _classesRepository;
        public ISectionRepository _sectionsRepository;
        public ISectionSubjectRepository _sectionSubjectsRepository;
        public ITimeTableRepository _timeTablesRepository;


        public UnitOfWork(SchoolManagementDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Repository Properties (shared DbContext)
        public IAuthRepository AuthRepository =>
            _authRepository ??= new AuthRepository(_context);

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

        #region SaveChanges (with safe error handling)
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log or rethrow with meaningful message
                throw new DbUpdateConcurrencyException(
                    "A concurrency conflict occurred while saving changes. " +
                    "The data may have been modified or deleted by another user.", ex);
            }
            catch (DbUpdateException ex)
            {
                // Capture any FK or constraint violation
                throw new InvalidOperationException("Database update failed.", ex);
            }
        }
        #endregion

        #region Transaction Handling
        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (_transaction != null)
                return; // Prevent nested transactions

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_transaction != null)
                {
                    await _context.SaveChangesAsync(); // Ensure pending changes are committed
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
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
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
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
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
