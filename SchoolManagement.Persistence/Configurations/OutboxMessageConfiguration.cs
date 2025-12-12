using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Persistence.Outbox;

namespace SchoolManagement.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(x => x.Id);

            // Let EF/DB generate Guid on insert
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            // EventId - Critical for idempotency
            builder.Property(x => x.EventId)
                .IsRequired();

            builder.Property(x => x.EventType)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Payload)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100);

            builder.Property(x => x.CausationId)
                .HasMaxLength(100);

            builder.Property(x => x.Source)
                .HasMaxLength(200);

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            builder.Property(x => x.Error)
                .HasMaxLength(4000)
                .IsRequired(false);

            builder.Property(x => x.RetryCount)
                .IsRequired(false);

            builder.Property(x => x.Metadata)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // Indexes for query performance
            builder.HasIndex(x => x.ProcessedAt)
                .HasDatabaseName("IX_OutboxMessages_ProcessedAt");

            builder.HasIndex(x => new { x.ProcessedAt, x.CreatedAt })
                .HasDatabaseName("IX_OutboxMessages_ProcessedAt_CreatedAt");

            builder.HasIndex(x => x.EventType)
                .HasDatabaseName("IX_OutboxMessages_EventType");

            builder.HasIndex(x => x.CorrelationId)
                .HasDatabaseName("IX_OutboxMessages_CorrelationId");

            // CRITICAL: Unique index on EventId for idempotency
            // Prevents duplicate events from being inserted
            builder.HasIndex(x => x.EventId)
                .IsUnique()
                .HasFilter("[EventId] IS NOT NULL")
                .HasDatabaseName("IX_OutboxMessages_EventId_Unique");

            // Table name
            builder.ToTable("OutboxMessages");
        }
    }
}