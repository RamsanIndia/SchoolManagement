using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Services
{
    public class ChangeTrackerService : IChangeTrackerService
    {
        private readonly SchoolManagementDbContext _context;

        public ChangeTrackerService(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public void Clear()
        {
            _context.ChangeTracker.Clear();
        }

        public async Task ReloadEntityAsync(object entity, CancellationToken cancellationToken = default)
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                await entry.ReloadAsync(cancellationToken);
            }
        }
    }
}
