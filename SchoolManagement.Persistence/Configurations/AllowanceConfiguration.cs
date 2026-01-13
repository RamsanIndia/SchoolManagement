using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Configurations
{
    public class AllowanceConfiguration : IEntityTypeConfiguration<Allowance>
    {
        public void Configure(EntityTypeBuilder<Allowance> builder)
        {
            builder.ToTable("Allowances");

            builder.HasKey(a => a.Id);

            // PostgreSQL: Guid maps to uuid automatically (no need to specify HasColumnType)
            builder.Property(a => a.Id)
                .IsRequired();

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200); // PostgreSQL: varchar(200)

            builder.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(1000); // PostgreSQL: varchar(1000)

            builder.Property(a => a.Type)
                .IsRequired()
                .HasConversion<int>(); // Enum stored as integer

            builder.Property(a => a.Amount)
                .IsRequired()
                .HasPrecision(18, 2); // PostgreSQL: numeric(18,2)

            builder.Property(a => a.IsPercentage)
                .IsRequired(); // PostgreSQL: boolean (default mapping)

            builder.Property(a => a.IsTaxable)
                .IsRequired(); // PostgreSQL: boolean

            builder.Property(a => a.IsActive)
                .IsRequired(); // PostgreSQL: boolean

            // BaseEntity inherited properties
            builder.Property(a => a.CreatedAt)
                .IsRequired(); // PostgreSQL: timestamp with time zone

            builder.Property(a => a.UpdatedAt)
                .IsRequired(false); // PostgreSQL: timestamp with time zone (nullable)

            builder.Property(a => a.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.UpdatedBy)
                .HasMaxLength(100);

            builder.Property(a => a.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(a => a.CreatedIP)
                .IsRequired()
                .HasMaxLength(45); // IPv6 compatible

            // IMPORTANT: Remove RowVersion for PostgreSQL
            // PostgreSQL doesn't have rowversion; if you need concurrency, use xmin
            // For now, ignore RowVersion property (don't map it, or use UseXminAsConcurrencyToken)
            builder.Ignore(a => a.RowVersion);

            // Optional: Use PostgreSQL xmin for concurrency instead
            // builder.Property(a => a.RowVersion)
            //     .IsRowVersion()
            //     .HasColumnType("xid")
            //     .ValueGeneratedOnAddOrUpdate();
        }
    }
}
