using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    // <summary>
    /// Base repository implementation providing common data access operations
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public abstract class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly SchoolManagementDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected Repository(SchoolManagementDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            return predicate == null
                ? await _dbSet.CountAsync(cancellationToken)
                : await _dbSet.CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Generic paginated query method
        /// </summary>
        //public virtual async Task<PagedResult<T>> GetPagedAsync(
        //    int pageNumber,
        //    int pageSize,
        //    Expression<Func<T, bool>>? predicate = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        //    CancellationToken cancellationToken = default)
        //{
        //    IQueryable<T> query = _dbSet.AsNoTracking();

        //    // Apply filter
        //    if (predicate != null)
        //        query = query.Where(predicate);

        //    // Apply ordering
        //    if (orderBy != null)
        //        query = orderBy(query);

        //    // Use extension method for pagination
        //    return await query.ToPagedResultAsync(pageNumber, pageSize);
        //}

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        // ===== COMMAND OPERATIONS =====

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.UpdateRange(entities);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await DeleteAsync(entity, cancellationToken);
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public virtual async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                // Assuming BaseEntity has an IsDeleted property
                // If not, you can add it or implement soft delete differently
                var entityType = entity.GetType();
                var isDeletedProperty = entityType.GetProperty("IsDeleted");

                if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
                {
                    isDeletedProperty.SetValue(entity, true);
                    await UpdateAsync(entity, cancellationToken);
                }
                else
                {
                    // If IsDeleted property doesn't exist, perform hard delete
                    await DeleteAsync(entity, cancellationToken);
                }
            }
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet.AsNoTracking();
        }
    }
}
