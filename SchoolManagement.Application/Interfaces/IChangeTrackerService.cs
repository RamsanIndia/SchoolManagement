using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IChangeTrackerService
    {
        void Clear();
        Task ReloadEntityAsync(object entity, CancellationToken cancellationToken = default);
    }
}
