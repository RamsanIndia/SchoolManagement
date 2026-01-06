using Microsoft.EntityFrameworkCore.Storage;

namespace SchoolManagement.Application.Interfaces;

/// <summary>
/// Manages database transactions with retry strategy support
/// </summary>
public interface ITransactionManager : IDisposable
{
    /// <summary>
    /// Gets the current active transaction
    /// </summary>
    IDbContextTransaction? GetCurrentTransaction();

    /// <summary>
    /// Indicates whether there is an active transaction
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Begins a new transaction
    /// Returns null if a transaction is already active (prevents nested transactions)
    /// </summary>
    Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction and saves all changes
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action within a transaction with automatic retry support
    /// This method handles transaction creation, commit, and rollback automatically
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}