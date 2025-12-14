using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Persistence.Outbox;

namespace SchoolManagement.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(x => x.Id);

            // Guid -> uuid automatically in PostgreSQL (no SQL Server types)
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.EventId).IsRequired();

            builder.Property(x => x.EventType)
                .HasMaxLength(500)
                .IsRequired();

            // nvarchar(max) doesn't exist in PostgreSQL; use text (or jsonb if you prefer)
            builder.Property(x => x.Payload)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.Metadata)
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CausationId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Source)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Error)
                .HasMaxLength(4000)
                .IsRequired(false);

            builder.Property(x => x.RetryCount)
                .IsRequired(false);

            // IMPORTANT: SQL Server rowversion doesn't exist in PostgreSQL.
            // Remove/ignore RowVersion for this table (or switch to xmin-based concurrency).
            builder.Ignore(x => x.RowVersion);

            // Indexes
            builder.HasIndex(x => x.ProcessedAt).HasDatabaseName("IX_OutboxMessages_ProcessedAt");
            builder.HasIndex(x => new { x.ProcessedAt, x.CreatedAt }).HasDatabaseName("IX_OutboxMessages_ProcessedAt_CreatedAt");
            builder.HasIndex(x => x.EventType).HasDatabaseName("IX_OutboxMessages_EventType");
            builder.HasIndex(x => x.CorrelationId).HasDatabaseName("IX_OutboxMessages_CorrelationId");

            // Unique index for idempotency
            // Since EventId is required, no filter is needed.
            builder.HasIndex(x => x.EventId)
                .IsUnique()
                .HasDatabaseName("IX_OutboxMessages_EventId_Unique");
        }
    }
}
