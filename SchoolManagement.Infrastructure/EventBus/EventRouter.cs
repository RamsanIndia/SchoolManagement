using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus
{
    public class EventRouter : IEventRouter
    {
        private readonly ConcurrentDictionary<string, Func<string, IServiceProvider, CancellationToken, Task>> _handlers = new();
        private readonly ILogger<EventRouter> _logger;

        public EventRouter(ILogger<EventRouter> logger)
        {
            _logger = logger;
        }

        // Register handlers dynamically
        public void RegisterHandler<TEvent>(Func<TEvent, IServiceProvider, CancellationToken, Task> handler)
            where TEvent : IDomainEvent
        {
            var eventType = typeof(TEvent);
            var eventTypeName = eventType.FullName;

            _handlers[eventTypeName] = async (eventData, serviceProvider, cancellationToken) =>
            {
                var @event = JsonSerializer.Deserialize<TEvent>(eventData);
                await handler(@event, serviceProvider, cancellationToken);
            };

            _logger.LogInformation("Registered handler for event type: {EventType}", eventTypeName);
        }

        // Route dynamically based on type name
        public async Task RouteAsync(
            string eventType,
            string eventData,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            if (_handlers.TryGetValue(eventType, out var handler))
            {
                await handler(eventData, serviceProvider, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No handler registered for event type: {EventType}", eventType);
            }
        }
    }
}
