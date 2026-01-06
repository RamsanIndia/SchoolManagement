using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence
{
    /// <summary>
    /// Transaction manager that works with EF Core retry strategy
    /// Handles database transactions with automatic retry support for transient failures
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<TransactionManager> _logger;
        private IDbContextTransaction? _currentTransaction;

        public TransactionManager(
            SchoolManagementDbContext context,
            ILogger<TransactionManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IDbContextTransaction? GetCurrentTransaction() => _currentTransaction;

        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // If already in a transaction, don't create a nested one
            if (_currentTransaction != null)
            {
                _logger.LogDebug("Transaction already active, skipping nested transaction");
                return null;
            }

            // Use the execution strategy to ensure transaction works with retry logic
            var strategy = _context.Database.CreateExecutionStrategy();

            _currentTransaction = await strategy.ExecuteAsync(async () =>
            {
                var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Transaction {TransactionId} started", transaction.TransactionId);
                return transaction;
            });

            return _currentTransaction;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Transaction {TransactionId} committed successfully",
                    _currentTransaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit transaction {TransactionId}",
                    _currentTransaction.TransactionId);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                _logger.LogWarning("Attempted to rollback but no active transaction exists");
                return;
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);

                _logger.LogWarning("Transaction {TransactionId} rolled back",
                    _currentTransaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction {TransactionId}",
                    _currentTransaction.TransactionId);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            // Use execution strategy to wrap the entire transaction
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    _logger.LogInformation("Executing action in transaction {TransactionId}",
                        transaction.TransactionId);

                    var result = await action();

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Transaction {TransactionId} completed successfully",
                        transaction.TransactionId);

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction {TransactionId} failed, rolling back",
                        transaction.TransactionId);

                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}
