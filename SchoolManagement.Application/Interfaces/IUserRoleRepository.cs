using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        
        Task<IEnumerable<UserRole>> GetAllWithUserAndRoleAsync(CancellationToken cancellationToken);
        Task<IEnumerable<UserRole>> FindWithUserAndRoleAsync(Expression<Func<UserRole, bool>> predicate, CancellationToken cancellationToken);

    }
}
