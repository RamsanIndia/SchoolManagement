using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserRole>> GetAllWithUserAndRoleAsync(CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserRole>> FindWithUserAndRoleAsync(Expression<Func<UserRole, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(predicate)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
