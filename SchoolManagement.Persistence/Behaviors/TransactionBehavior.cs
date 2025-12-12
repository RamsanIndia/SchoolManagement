using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IChangeTrackerService _changeTrackerService;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(
            ITransactionManager transactionManager,
            IChangeTrackerService changeTrackerService,
            ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _transactionManager = transactionManager;
            _changeTrackerService = changeTrackerService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            if (requestName.EndsWith("Query"))
            {
                return await next();
            }

            await _transactionManager.BeginTransactionAsync(cancellationToken);

            try
            {
                var response = await next();
                await _transactionManager.CommitAsync(cancellationToken);
                return response;
            }
            catch (Exception)
            {
                await _transactionManager.RollbackAsync(cancellationToken);

                // Use abstraction instead of direct DbContext access
                _changeTrackerService.Clear();

                throw;
            }
        }
    }
}
