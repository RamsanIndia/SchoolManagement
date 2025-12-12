using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IAuthRepository AuthRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        IEmployeeRepository EmployeeRepository { get; }
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        IMenuRepository MenuRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        IStudentRepository StudentRepository { get; }
        IAttendanceRepository AttendanceRepository { get; }
        IRoleMenuPermissionRepository RoleMenuPermissionRepository { get; } 
        IClassRepository ClassesRepository { get; }
        ISectionRepository SectionsRepository { get; }
        ISectionSubjectRepository SectionSubjectsRepository { get; }
        ITimeTableRepository TimeTablesRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IPermissionRepository Permissions { get; }
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }
}
