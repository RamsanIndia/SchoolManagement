using SchoolManagement.Domain.Common;
using System.Linq.Expressions;

namespace SchoolManagement.Application.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// Follows Repository Pattern and Unit of Work pattern.
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        
        /// <summary>
        /// Retrieves an entity by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities of type T
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first entity that matches the predicate or null if none found
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First matching entity or null</returns>
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the single entity that matches the predicate or throws if multiple/none found
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Single matching entity</returns>
        /// <exception cref="InvalidOperationException">Thrown when zero or multiple entities match</exception>
        Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an entity with the specified ID exists
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the specified predicate
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any entity matches, false otherwise</returns>
        Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities matching the optional predicate
        /// </summary>
        /// <param name="predicate">Optional filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of matching entities</returns>
        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        IQueryable<T> GetQueryable();

        /// <summary>
        /// Returns a paginated result set
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="predicate">Optional filter expression</param>
        /// <param name="orderBy">Optional ordering function</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated result</returns>
        //Task<PagedResult<T>> GetPagedAsync(
        //    int pageNumber,
        //    int pageSize,
        //    Expression<Func<T, bool>>? predicate = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        //    CancellationToken cancellationToken = default);

        /// <summary>
        /// Provides direct access to IQueryable for complex queries
        /// Use with caution - prefer other methods when possible
        /// </summary>
        /// <returns>IQueryable of entity type</returns>
        IQueryable<T> Query();

        
        /// <summary>
        /// Adds a new entity to the repository
        /// Note: Call SaveChangesAsync on UnitOfWork to persist changes
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple entities to the repository
        /// Note: Call SaveChangesAsync on UnitOfWork to persist changes
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity
        /// Note: Call SaveChangesAsync on UnitOfWork to persist changes
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple entities
        /// Note: Call SaveChangesAsync on UnitOfWork to persist changes
        /// </summary>
        /// <param name="entities">Entities to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity from the repository
        /// Note: Call SaveChangesAsync on UnitOfWork to persist changes
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity by its ID
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes multiple entities
        /// Note: Call SaveChangesAsync on UnitOfWork to persist changes
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes an entity (sets IsDeleted flag if available)
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
