using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Persistence.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence
{
    /// <summary>
    /// DbContext with Transactional Outbox Pattern
    /// </summary>
    public class SchoolManagementDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SchoolManagementDbContext> _logger;
        private readonly IpAddressHelper _ipAddressHelper;

        public SchoolManagementDbContext(
            DbContextOptions<SchoolManagementDbContext> options,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SchoolManagementDbContext> logger,
            IpAddressHelper ipAddressHelper)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _ipAddressHelper = ipAddressHelper;
        }

        #region DbSets

        // Master Data
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Academic
        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionSubject> SectionSubjects { get; set; }
        public DbSet<TimeTableEntry> TimeTableEntries { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<StudentParent> StudentParents { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }

        // Employee Data
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendances { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<PayrollRecord> PayrollRecords { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<Allowance> Allowances { get; set; }
        public DbSet<Deduction> Deductions { get; set; }

        // Outbox for reliable event publishing
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolManagementDbContext).Assembly);


            // RowVersion concurrency for BaseEntity types
            //foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            //             .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            //{
            //    modelBuilder.Entity(entityType.ClrType)
            //        .Property(nameof(BaseEntity.RowVersion))
            //        .IsRowVersion()
            //        .IsConcurrencyToken();
            //}

            // Configure RowVersion to map to xmin for all BaseEntity types
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Synchronous SaveChanges - not recommended for production
        /// Use SaveChangesAsync instead
        /// </summary>
        //public override int SaveChanges()
        //{
        //    _logger.LogWarning("Synchronous SaveChanges called. Consider using SaveChangesAsync for better performance.");
        //    return SaveChanges(acceptAllChangesOnSuccess: true);
        //}

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                ApplyAuditInfo();
                var domainEvents = CollectDomainEvents();

                if (domainEvents.Any())
                {
                    AddEventsToOutbox(domainEvents);
                }

                ClearDomainEvents();
                var result = base.SaveChanges(acceptAllChangesOnSuccess);

                _logger.LogInformation(
                    "Saved {EntityCount} entities and {EventCount} outbox messages",
                    result,
                    domainEvents.Count
                );

                return result;
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyError(ex))
            {
                _logger.LogWarning(ex, "❌ Duplicate key error during SaveChanges");
                return HandleDuplicateKeyError(ex, acceptAllChangesOnSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SaveChanges");
                CleanupFailedOutboxMessages();
                throw;
            }
        }

        /// <summary>
        /// SaveChangesAsync with Transactional Outbox Pattern and Idempotency
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                // Step 1: Apply audit information
                ApplyAuditInfo();

                // Step 2: Collect domain events BEFORE saving
                var domainEvents = CollectDomainEvents();

                // Step 3: Add events to outbox (tracked by EF, not yet saved)
                if (domainEvents.Any())
                {
                    await AddEventsToOutboxAsync(domainEvents, cancellationToken);
                }

                // Step 4: Clear events from entities immediately after capturing
                ClearDomainEvents();

                // Step 5: Single atomic save - saves both entities and outbox messages
                var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

                _logger.LogInformation(
                    "✅ Saved {EntityCount} entities and {EventCount} outbox messages",
                    result,
                    domainEvents.Count
                );

                return result;
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyError(ex))
            {
                _logger.LogWarning(ex, "❌ Duplicate key error during SaveChangesAsync - Handling idempotently");
                return await HandleDuplicateKeyErrorAsync(ex, acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict occurred. Consider implementing retry logic at the handler level.");
                CleanupFailedOutboxMessages();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SaveChangesAsync");
                CleanupFailedOutboxMessages();
                throw;
            }
        }

        /// <summary>
        /// Check if exception is a duplicate key error for OutboxMessages
        /// </summary>
        //private bool IsDuplicateKeyError(DbUpdateException ex)
        //{
        //    if (ex.InnerException is SqlException sqlEx)
        //    {
        //        // 2627 = Unique constraint violation
        //        // 2601 = Duplicate key row error
        //        return (sqlEx.Number == 2627 || sqlEx.Number == 2601)
        //            && (sqlEx.Message.Contains("PK_OutboxMessages")
        //                || sqlEx.Message.Contains("IX_OutboxMessages_EventId_Unique"));
        //    }
        //    return false;
        //}

        private bool IsDuplicateKeyError(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                // 23505 = unique_violation
                return pgEx.SqlState == "23505"
                       && (pgEx.ConstraintName == "PK_OutboxMessages"
                           || pgEx.ConstraintName == "IX_OutboxMessages_EventId_Unique");
            }

            // Optional: keep SQL Server handling if you still run SQL Server in dev
            // if (ex.InnerException is SqlException sqlEx)
            //     return (sqlEx.Number == 2627 || sqlEx.Number == 2601);

            return false;
        }

        /// <summary>
        /// Handle duplicate key error idempotently (synchronous)
        /// </summary>
        private int HandleDuplicateKeyError(DbUpdateException ex, bool acceptAllChangesOnSuccess)
        {
            // Get the duplicate outbox messages
            var duplicateOutboxEntries = ChangeTracker.Entries<OutboxMessage>()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            if (!duplicateOutboxEntries.Any())
            {
                _logger.LogWarning("Duplicate key error but no outbox messages found in change tracker");
                throw ex;
            }

            // Log duplicate message IDs
            var duplicateIds = duplicateOutboxEntries.Select(e => e.Entity.Id).ToList();
            _logger.LogWarning(
                "Found {Count} duplicate outbox messages: {MessageIds}. Detaching and retrying.",
                duplicateIds.Count,
                string.Join(", ", duplicateIds)
            );

            // Detach all outbox messages from change tracker
            foreach (var entry in duplicateOutboxEntries)
            {
                entry.State = EntityState.Detached;
            }

            // Check if there are still other entities to save
            var remainingChanges = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .Count();

            if (remainingChanges > 0)
            {
                _logger.LogInformation(
                    "Retrying SaveChanges for {RemainingCount} remaining entities",
                    remainingChanges
                );
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }

            _logger.LogInformation("No remaining changes after detaching duplicate outbox messages");
            return 0;
        }

        /// <summary>
        /// Handle duplicate key error idempotently (asynchronous)
        /// </summary>
        private async Task<int> HandleDuplicateKeyErrorAsync(
            DbUpdateException ex,
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken)
        {
            // Get the duplicate outbox messages
            var duplicateOutboxEntries = ChangeTracker.Entries<OutboxMessage>()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            if (!duplicateOutboxEntries.Any())
            {
                _logger.LogWarning("Duplicate key error but no outbox messages found in change tracker");
                throw ex;
            }

            // Log duplicate message IDs
            var duplicateIds = duplicateOutboxEntries.Select(e => e.Entity.Id).ToList();
            _logger.LogWarning(
                "Found {Count} duplicate outbox messages: {MessageIds}. Detaching and retrying.",
                duplicateIds.Count,
                string.Join(", ", duplicateIds)
            );

            // Detach all outbox messages from change tracker
            foreach (var entry in duplicateOutboxEntries)
            {
                entry.State = EntityState.Detached;
            }

            // Check if there are still other entities to save
            var remainingChanges = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .Count();

            if (remainingChanges > 0)
            {
                _logger.LogInformation(
                    "Retrying SaveChangesAsync for {RemainingCount} remaining entities",
                    remainingChanges
                );
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            _logger.LogInformation("No remaining changes after detaching duplicate outbox messages");
            return 0;
        }

        /// <summary>
        /// Clean up failed outbox messages from change tracker
        /// </summary>
        private void CleanupFailedOutboxMessages()
        {
            var outboxEntries = ChangeTracker.Entries<OutboxMessage>()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            if (outboxEntries.Any())
            {
                _logger.LogWarning(
                    "Cleaning up {Count} failed outbox messages from change tracker",
                    outboxEntries.Count
                );

                foreach (var entry in outboxEntries)
                {
                    entry.State = EntityState.Detached;
                }
            }
        }

        /// <summary>
        /// Collect domain events from all tracked entities
        /// Excludes OutboxMessage entities to prevent infinite loops
        /// </summary>
        private List<IDomainEvent> CollectDomainEvents()
        {
            var domainEvents = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any()
                            && e.Entity.GetType() != typeof(OutboxMessage))
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            return domainEvents;
        }

        /// <summary>
        /// Add domain events to outbox table with duplicate check (synchronous)
        /// </summary>
        private void AddEventsToOutbox(List<IDomainEvent> domainEvents)
        {
            var correlationId = _ipAddressHelper.GetCorrelationId();
            var userId = _ipAddressHelper.GetCurrentUserId();
            var ipAddress = _ipAddressHelper.GetClientIpAddress();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            foreach (var domainEvent in domainEvents)
            {
                var eventId = domainEvent.EventId != Guid.Empty
                    ? domainEvent.EventId
                    : Guid.NewGuid();

                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    EventType = domainEvent.GetType().FullName,
                    Payload = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = null,
                    CorrelationId = correlationId,
                    CausationId = domainEvent.EventId.ToString(),
                    Source = "SchoolManagementAPI",
                    Metadata = JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        Timestamp = DateTime.UtcNow,
                        Environment = environment,
                        MachineName = Environment.MachineName,
                        IpAddress = ipAddress
                    })
                };

                OutboxMessages.Add(outboxMessage);
            }

            _logger.LogDebug(
                "Prepared {EventCount} events for outbox with correlation ID {CorrelationId}",
                domainEvents.Count,
                correlationId
            );
        }

        /// <summary>
        /// Add domain events to outbox table with duplicate check (asynchronous)
        /// </summary>
        private async Task AddEventsToOutboxAsync(
            List<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            var correlationId = _ipAddressHelper.GetCorrelationId();
            var userId = _ipAddressHelper.GetCurrentUserId();
            var ipAddress = _ipAddressHelper.GetClientIpAddress();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            // Get all event IDs to check for duplicates
            var eventIds = domainEvents
                .Select(e => e.EventId != Guid.Empty ? e.EventId : Guid.NewGuid())
                .ToList();

            // Check which events already exist in database
            var existingEventIds = await OutboxMessages
                .Where(m => eventIds.Contains(m.EventId))
                .Select(m => m.EventId)
                .ToListAsync(cancellationToken);

            var addedCount = 0;
            var skippedCount = 0;

            foreach (var domainEvent in domainEvents)
            {
                var eventId = domainEvent.EventId != Guid.Empty
                    ? domainEvent.EventId
                    : Guid.NewGuid();

                // Skip if event already exists in database
                if (existingEventIds.Contains(eventId))
                {
                    _logger.LogDebug(
                        "Skipping duplicate event {EventType} with EventId {EventId}",
                        domainEvent.GetType().Name,
                        eventId
                    );
                    skippedCount++;
                    continue;
                }

                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    EventType = domainEvent.GetType().FullName,
                    Payload = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = null,
                    CorrelationId = correlationId,
                    CausationId = domainEvent.EventId.ToString(),
                    Source = "SchoolManagementAPI",
                    Metadata = JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        Timestamp = DateTime.UtcNow,
                        Environment = environment,
                        MachineName = Environment.MachineName,
                        IpAddress = ipAddress
                    })
                };

                OutboxMessages.Add(outboxMessage);
                addedCount++;
            }

            if (skippedCount > 0)
            {
                _logger.LogInformation(
                    "Prepared {AddedCount} new events, skipped {SkippedCount} duplicate events for outbox with correlation ID {CorrelationId}",
                    addedCount,
                    skippedCount,
                    correlationId
                );
            }
            else
            {
                _logger.LogDebug(
                    "Prepared {EventCount} events for outbox with correlation ID {CorrelationId}",
                    addedCount,
                    correlationId
                );
            }
        }

        /// <summary>
        /// Clear domain events from all entities
        /// </summary>
        private void ClearDomainEvents()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }

        /// <summary>
        /// Apply audit information to entities
        /// </summary>
        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            var currentUser = _ipAddressHelper.GetCurrentUserId();
            var currentIp = _ipAddressHelper.GetClientIpAddress();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.SetCreated(currentUser, currentIp);
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.SetUpdated(currentUser, currentIp);
                }
            }
        }

        ///// <summary>
        ///// Get current user ID from HTTP context
        ///// </summary>
        //private string GetCurrentUserId()
        //{
        //    return _httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value
        //           ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("userId")?.Value
        //           ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name
        //           ?? "System";
        //}

        ///// <summary>
        ///// Get current IP address from HTTP context
        ///// </summary>
        //private string GetCurrentIpAddress()
        //{
        //    return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        //           ?? "Unknown";
        //}

        ///// <summary>
        ///// Get or generate correlation ID for request tracing
        ///// </summary>
        //private string GetCorrelationId()
        //{
        //    var correlationId = _httpContextAccessor?.HttpContext?.TraceIdentifier;

        //    if (string.IsNullOrEmpty(correlationId))
        //    {
        //        correlationId = _httpContextAccessor?.HttpContext?.Request?.Headers["X-Correlation-ID"]
        //            .FirstOrDefault();
        //    }

        //    return correlationId ?? Guid.NewGuid().ToString();
        //}
    }
}