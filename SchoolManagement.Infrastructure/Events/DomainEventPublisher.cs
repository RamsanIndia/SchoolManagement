//// Infrastructure/Events/DomainEventPublisher.cs
//using MediatR;
//using Microsoft.Extensions.Logging;
//using SchoolManagement.Domain.Common;
//using SchoolManagement.Infrastructure.EventBus;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SchoolManagement.Infrastructure.Events
//{
//    /// <summary>
//    /// Publishes domain events to external event bus (RabbitMQ, Azure Service Bus, etc.)
//    /// This is used for cross-bounded context communication and integration events
//    /// </summary>
//    public class DomainEventPublisher : IDomainEventPublisher
//    {
//        private readonly IEventBus _eventBus;
//        private readonly IIntegrationEventMapper _eventMapper;
//        private readonly ILogger<DomainEventPublisher> _logger;

//        public DomainEventPublisher(
//            IEventBus eventBus,
//            IIntegrationEventMapper eventMapper,
//            ILogger<DomainEventPublisher> logger)
//        {
//            _eventBus = eventBus;
//            _eventMapper = eventMapper;
//            _logger = logger;
//        }

//        public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
//        {
//            if (domainEvent == null)
//            {
//                _logger.LogWarning("Attempted to publish null domain event");
//                return;
//            }

//            try
//            {
//                // Map domain event to integration event
//                var integrationEvent = _eventMapper.MapToIntegrationEvent(domainEvent);

//                // Only publish if there's a corresponding integration event
//                if (integrationEvent == null)
//                {
//                    _logger.LogDebug(
//                        "Domain event {EventType} does not map to integration event, skipping external publish",
//                        domainEvent.GetType().Name
//                    );
//                    return;
//                }

//                // Publish to external event bus
//                await _eventBus.PublishAsync(integrationEvent);

//                _logger.LogInformation(
//                    "Domain event {EventType} published as integration event {IntegrationEventType} at {OccurredOn}",
//                    domainEvent.GetType().Name,
//                    integrationEvent.GetType().Name,
//                    domainEvent.OccurredOn
//                );
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(
//                    ex,
//                    "Failed to publish domain event {EventType} that occurred at {OccurredOn}",
//                    domainEvent.GetType().Name,
//                    domainEvent.OccurredOn
//                );

//                // Optionally: Store failed event for retry
//                // await _outboxRepository.SaveFailedEventAsync(domainEvent, ex.Message);

//                throw;
//            }
//        }

//        public async Task PublishManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
//        {
//            if (domainEvents == null || !domainEvents.Any())
//            {
//                return;
//            }

//            foreach (var domainEvent in domainEvents)
//            {
//                await PublishAsync(domainEvent, cancellationToken);
//            }
//        }
//    }
//}
