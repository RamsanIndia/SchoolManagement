using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SchoolManagement.Infrastructure.Persistence.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            // ===== TABLE =====
            builder.ToTable("Tenants", "dbo");

            // ===== PRIMARY KEY =====
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .IsRequired()
                .ValueGeneratedNever(); // Generated in domain

            // ===== TENANT PROPERTIES =====

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            //builder.Property(t => t.ConnectionString)
            //    .HasMaxLength(500)
            //    .HasColumnType("varchar(500)");

            builder.Property(t => t.Plan)
                .IsRequired()
                .HasConversion<string>() // Store as string in DB
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .HasDefaultValue(TenantPlan.Basic);

            builder.Property(t => t.SubscriptionExpiryDate)
                .HasColumnType("timestamp with time zone");

            builder.Property(t => t.MaxSchools)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(t => t.MaxUsers)
                .IsRequired()
                .HasDefaultValue(50);

            // ===== BASE ENTITY PROPERTIES =====

            // TenantId - NULL for Tenant entity (it IS the tenant)
            builder.Property(t => t.TenantId)
                .IsRequired(false);

            // SchoolId - NULL for Tenant entity
            builder.Property(t => t.SchoolId)
                .IsRequired(false);

            // Audit fields
            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(t => t.CreatedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(t => t.CreatedIP)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(t => t.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(t => t.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(t => t.UpdatedIP)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // Soft delete
            builder.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.DeletedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(t => t.DeletedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            // Active status
            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Row version for concurrency
            builder.Property(s => s.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

            // ===== INDEXES =====

            // Unique index on Code
            builder.HasIndex(t => t.Code)
                .IsUnique()
                .HasDatabaseName("IX_Tenants_Code");

            // Index on Name for searching
            builder.HasIndex(t => t.Name)
                .HasDatabaseName("IX_Tenants_Name");

            // Index on IsActive for filtering
            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_Tenants_IsActive");

            // Index on IsDeleted for soft delete filtering
            builder.HasIndex(t => t.IsDeleted)
                .HasDatabaseName("IX_Tenants_IsDeleted");

            // Composite index for active non-deleted tenants
            builder.HasIndex(t => new { t.IsActive, t.IsDeleted })
                .HasDatabaseName("IX_Tenants_IsActive_IsDeleted");

            // Index on SubscriptionExpiryDate for subscription checks
            builder.HasIndex(t => t.SubscriptionExpiryDate)
                .HasDatabaseName("IX_Tenants_SubscriptionExpiryDate");

            // ===== RELATIONSHIPS =====

            // Tenant has many Schools
            builder.HasMany(t => t.Schools)
                .WithOne()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== QUERY FILTERS =====

            // Global query filter for soft delete
            builder.HasQueryFilter(t => !t.IsDeleted);

            // ===== IGNORE PROPERTIES =====

            // Ignore navigation properties that are loaded separately
            builder.Ignore(t => t.CurrentSchoolCount);
            builder.Ignore(t => t.IsSubscriptionActive);

            // Ignore domain events (not persisted)
            builder.Ignore(t => t.DomainEvents);

            // ===== SEED DATA (Optional) =====
            // Uncomment to seed initial tenant data
            /*
            builder.HasData(
                new 
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "Demo Tenant",
                    Code = "DEMO",
                    Plan = TenantPlan.Premium,
                    MaxSchools = 10,
                    MaxUsers = 500,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            );
            */
        }
    }

}
