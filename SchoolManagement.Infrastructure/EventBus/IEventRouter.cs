using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus
{
    public interface IEventRouter
    {
        void RegisterHandler<TEvent>(Func<TEvent, IServiceProvider, CancellationToken, Task> handler)
            where TEvent : IDomainEvent;
        Task RouteAsync(string eventType, string eventData, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
