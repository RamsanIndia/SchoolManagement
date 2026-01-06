using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;

namespace SchoolManagement.Persistence.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps command handlers in database transactions
/// Automatically manages transaction lifecycle (begin, commit, rollback)
/// Skips transactions for queries (read-only operations)
/// Uses execution strategy to support retry logic with transactions
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITransactionManager _transactionManager;
    private readonly IChangeTrackerService _changeTrackerService;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    private readonly SchoolManagementDbContext _dbContext;

    public TransactionBehavior(
        ITransactionManager transactionManager,
        IChangeTrackerService changeTrackerService,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger,
        SchoolManagementDbContext dbContext)
    {
        _transactionManager = transactionManager;
        _changeTrackerService = changeTrackerService;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction for queries (read-only operations)
        if (IsQuery(requestName))
        {
            _logger.LogDebug("Skipping transaction for query: {RequestName}", requestName);
            return await next();
        }

        // If already in a transaction, don't create a nested one
        if (_transactionManager.HasActiveTransaction)
        {
            _logger.LogDebug("Already in transaction, executing {RequestName} without nested transaction",
                requestName);
            return await next();
        }

        _logger.LogInformation("Beginning transaction for command: {RequestName}", requestName);

        // Create execution strategy to wrap transaction for retry support
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Begin transaction within execution strategy
            var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);

            // If transaction is null, we're in a nested call - just execute
            if (transaction == null)
            {
                _logger.LogDebug("Transaction already active, executing {RequestName}", requestName);
                return await next();
            }

            try
            {
                // Execute the handler
                var response = await next();

                // Commit transaction and save changes
                await _transactionManager.CommitAsync(cancellationToken);

                _logger.LogInformation("Transaction committed successfully for command: {RequestName}",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed for command: {RequestName}, rolling back",
                    requestName);

                // Rollback transaction
                await _transactionManager.RollbackAsync(cancellationToken);

                // Clear change tracker to prevent stale data
                _changeTrackerService.Clear();

                throw;
            }
        });
    }

    /// <summary>
    /// Determines if the request is a query (read-only operation)
    /// Queries don't need transactions
    /// </summary>
    private static bool IsQuery(string requestName)
    {
        return requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Get", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("List", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Search", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Find", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Fetch", StringComparison.OrdinalIgnoreCase) ||
               requestName.StartsWith("Get", StringComparison.OrdinalIgnoreCase);
    }
}
