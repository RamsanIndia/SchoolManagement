using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Services
{
    /// <summary>
    /// Implementation of IChangeTrackerService for managing EF Core change tracker
    /// </summary>
    public class ChangeTrackerService : IChangeTrackerService
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<ChangeTrackerService> _logger;

        public ChangeTrackerService(
            SchoolManagementDbContext context,
            ILogger<ChangeTrackerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Clear()
        {
            var trackedEntitiesCount = _context.ChangeTracker.Entries().Count();

            if (trackedEntitiesCount > 0)
            {
                _logger.LogInformation("Clearing {Count} tracked entities from change tracker",
                    trackedEntitiesCount);

                _context.ChangeTracker.Clear();

                _logger.LogDebug("Change tracker cleared successfully");
            }
        }

        public int GetTrackedEntitiesCount()
        {
            return _context.ChangeTracker.Entries().Count();
        }

        public void DetachAll()
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }

            _logger.LogInformation("Detached {Count} entities from change tracker", entries.Count);
        }

        /// <summary>
        /// Reloads an entity from the database, refreshing its values
        /// Useful when you need to discard local changes and get the latest data from database
        /// </summary>
        /// <param name="entity">The entity to reload</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ReloadEntityAsync(object entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entry = _context.Entry(entity);

            // Can't reload a detached entity
            if (entry.State == EntityState.Detached)
            {
                _logger.LogWarning("Cannot reload detached entity of type {EntityType}",
                    entity.GetType().Name);
                return;
            }

            _logger.LogDebug("Reloading entity of type {EntityType} from database",
                entity.GetType().Name);

            // Reload the entity from database
            await entry.ReloadAsync(cancellationToken);

            _logger.LogDebug("Entity of type {EntityType} reloaded successfully",
                entity.GetType().Name);
        }
    }
}
