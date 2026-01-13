using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            // ✅ Ignore BaseEntity fields not needed
            builder.Ignore(e => e.CreatedBy);
            builder.Ignore(e => e.CreatedIP);
            builder.Ignore(e => e.UpdatedAt);
            builder.Ignore(e => e.UpdatedBy);
            builder.Ignore(e => e.UpdatedIP);
            builder.Ignore(e => e.DeletedAt);
            builder.Ignore(e => e.DeletedBy);
            builder.Ignore(e => e.IsActive);
            builder.Ignore(e => e.IsDeleted);
            builder.Ignore(e => e.RowVersion);
            builder.Ignore(e => e.DomainEvents);

            // ✅ DECISION: Use only Timestamp property, ignore BaseEntity.CreatedAt
            builder.Ignore(e => e.CreatedAt);  // NEW: Prevent duplicate

            builder.Property(e => e.Timestamp)
                .HasColumnName("Timestamp")  // Explicit column name
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // Action enum
            builder.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            // Entity information
            builder.Property(e => e.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(100);

            // User information
            builder.Property(e => e.UserId).IsRequired(false);
            builder.Property(e => e.UserEmail).HasMaxLength(256).IsRequired(false);

            // Request context
            builder.Property(e => e.IpAddress).HasMaxLength(45).IsRequired(false);
            builder.Property(e => e.UserAgent).HasMaxLength(500).IsRequired(false);

            // Change tracking (JSON)
            builder.Property(e => e.OldValues).HasColumnType("jsonb").IsRequired(false);
            builder.Property(e => e.NewValues).HasColumnType("jsonb").IsRequired(false);
            builder.Property(e => e.ChangedFields).HasMaxLength(1000).IsRequired(false);

            // Performance metrics
            builder.Property(e => e.Duration).IsRequired(false);
            builder.Property(e => e.Status).HasMaxLength(50).IsRequired(false);

            // Indexes (unchanged)
            builder.HasIndex(e => e.UserId).HasDatabaseName("IX_AuditLogs_UserId");
            builder.HasIndex(e => e.EntityName).HasDatabaseName("IX_AuditLogs_EntityName");
            builder.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp").IsDescending();
            builder.HasIndex(e => e.Action).HasDatabaseName("IX_AuditLogs_Action");

            builder.HasIndex(e => new { e.EntityName, e.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityName_EntityId");
            builder.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("IX_AuditLogs_UserId_Timestamp").IsDescending();
            builder.HasIndex(e => new { e.EntityName, e.Action, e.Timestamp })
                .HasDatabaseName("IX_AuditLogs_EntityName_Action_Timestamp");

            builder.ToTable(t => t.HasComment(
                "Audit log table - Consider partitioning by Timestamp for large datasets"));
        }
    }
}
