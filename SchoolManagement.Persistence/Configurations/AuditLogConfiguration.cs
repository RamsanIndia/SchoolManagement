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

            // Primary Key
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever(); // GUID generated in domain

            // *** IMPORTANT: Ignore BaseEntity fields not needed for AuditLog ***
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

            // Map CreatedAt from BaseEntity to Timestamp column
            builder.Property(e => e.CreatedAt)
                .HasColumnName("Timestamp")
                .IsRequired();

            // Action enum stored as string
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
            builder.Property(e => e.UserId)
                .IsRequired(false);

            builder.Property(e => e.UserEmail)
                .HasMaxLength(256)
                .IsRequired(false);

            // Request context
            builder.Property(e => e.IpAddress)
                .HasMaxLength(45) // Supports IPv6
                .IsRequired(false);

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .IsRequired(false);

            // Timestamp (mapped from CreatedAt above)
            builder.Property(e => e.Timestamp)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // Change tracking (JSON columns for PostgreSQL)
            builder.Property(e => e.OldValues)
                .HasColumnType("jsonb")
                .IsRequired(false);

            builder.Property(e => e.NewValues)
                .HasColumnType("jsonb")
                .IsRequired(false);

            builder.Property(e => e.ChangedFields)
                .HasMaxLength(1000)
                .IsRequired(false);

            // Performance metrics
            builder.Property(e => e.Duration)
                .IsRequired(false);

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired(false);

            // Indexes for query performance
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(e => e.EntityName)
                .HasDatabaseName("IX_AuditLogs_EntityName");

            builder.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp")
                .IsDescending(); // Most recent first

            builder.HasIndex(e => e.Action)
                .HasDatabaseName("IX_AuditLogs_Action");

            // Composite indexes for common queries
            builder.HasIndex(e => new { e.EntityName, e.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityName_EntityId");

            builder.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("IX_AuditLogs_UserId_Timestamp")
                .IsDescending(); // For user activity history

            builder.HasIndex(e => new { e.EntityName, e.Action, e.Timestamp })
                .HasDatabaseName("IX_AuditLogs_EntityName_Action_Timestamp");

            // Partitioning hint comment (for future implementation)
            builder.ToTable(t => t.HasComment(
                "Audit log table - Consider partitioning by Timestamp for large datasets"));
        }
    }
}
