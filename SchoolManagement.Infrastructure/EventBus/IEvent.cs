using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
