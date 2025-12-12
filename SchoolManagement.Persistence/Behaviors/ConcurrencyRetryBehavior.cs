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
    // <summary>
    /// Pipeline behavior that automatically retries commands when concurrency conflicts occur
    /// </summary>
    public class ConcurrencyRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<ConcurrencyRetryBehavior<TRequest, TResponse>> _logger;
        private readonly IChangeTrackerService _changeTrackerService;
        private const int MaxRetries = 3;
        private const int BaseDelayMilliseconds = 100;

        public ConcurrencyRetryBehavior(
            ILogger<ConcurrencyRetryBehavior<TRequest, TResponse>> logger,
            IChangeTrackerService changeTrackerService)
        {
            _logger = logger;
            _changeTrackerService = changeTrackerService;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var retryCount = 0;

            while (true)
            {
                try
                {
                    return await next();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;

                    if (retryCount >= MaxRetries)
                    {
                        throw new InvalidOperationException(
                            $"Unable to complete the operation due to concurrent modifications. Please try again.",
                            ex);
                    }

                    _logger.LogWarning("Concurrency conflict for {RequestName}. Retry {RetryCount}/{MaxRetries}",
                        requestName, retryCount, MaxRetries);

                    // Use abstraction instead of direct DbContext access
                    _changeTrackerService.Clear();

                    var delay = BaseDelayMilliseconds * retryCount;
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}
