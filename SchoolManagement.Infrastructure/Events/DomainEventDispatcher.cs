//using SchoolManagement.Application.Abstractions.Events;
//using SchoolManagement.Domain.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SchoolManagement.Infrastructure.Events
//{
//    public class DomainEventDispatcher : IDomainEventDispatcher
//    {
//        private readonly IDomainEventPublisher _publisher;

//        public DomainEventDispatcher(IDomainEventPublisher publisher)
//        {
//            _publisher = publisher;
//        }

//        public async Task DispatchAsync(IEnumerable<BaseEntity> entitiesWithEvents, CancellationToken cancellationToken = default)
//        {
//            var events = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();

//            foreach (var @event in events)
//            {
//                await _publisher.PublishAsync(@event, cancellationToken);
//            }

//            foreach (var entity in entitiesWithEvents)
//            {
//                entity.ClearDomainEvents();
//            }
//        }
//    }
//}
