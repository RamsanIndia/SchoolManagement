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
    public class TransactionManager : ITransactionManager
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<TransactionManager> _logger;
        private IDbContextTransaction _currentTransaction;

        public TransactionManager(
            SchoolManagementDbContext context,
            ILogger<TransactionManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                _logger.LogDebug("Transaction already active, skipping BeginTransaction");
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogDebug("🔵 Transaction started: {TransactionId}", _currentTransaction.TransactionId);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                _logger.LogWarning("⚠️ CommitAsync called but no active transaction");
                return;
            }

            try
            {
                // IMPORTANT: Don't call SaveChangesAsync here!
                // SaveChanges should be called BEFORE CommitAsync
                // CommitAsync only commits the transaction that wraps the already-saved changes
                await _currentTransaction.CommitAsync(cancellationToken);
                _logger.LogDebug("✅ Transaction committed: {TransactionId}", _currentTransaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error committing transaction");
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                _logger.LogWarning("⚠️ RollbackAsync called but no active transaction");
                return;
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                _logger.LogDebug("🔙 Transaction rolled back: {TransactionId}", _currentTransaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error rolling back transaction");
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;

                // Clear change tracker to prevent stale state
                _context.ChangeTracker.Clear();
                _logger.LogDebug("🧹 ChangeTracker cleared after rollback");
            }
        }

        public bool HasActiveTransaction => _currentTransaction != null;
    }
}
