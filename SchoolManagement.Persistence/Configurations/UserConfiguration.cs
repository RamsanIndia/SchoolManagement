using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;

namespace SchoolManagement.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);

            // Email Value Object with Converter and Comparer
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasConversion(
                      email => email.Value,
                      value => new Email(value));

            entity.Property(e => e.Email)
                  .Metadata.SetValueComparer(
                      new ValueComparer<Email>(
                          (left, right) => left != null && right != null && left.Value == right.Value,
                          email => email != null ? email.Value.GetHashCode() : 0,
                          email => new Email(email.Value)));

            // FullName Value Object as Owned Type
            entity.OwnsOne(e => e.FullName, fullName =>
            {
                fullName.Property(fn => fn.FirstName)
                        .HasColumnName("FirstName")
                        .HasMaxLength(50)
                        .IsRequired();

                fullName.Property(fn => fn.LastName)
                        .HasColumnName("LastName")
                        .HasMaxLength(50)
                        .IsRequired();
            });

            // PhoneNumber Value Object (nullable)
            entity.Property(e => e.PhoneNumber)
                  .HasMaxLength(20)
                  .HasConversion(
                      phone => phone != null ? phone.Value : null,
                      value => value != null ? new PhoneNumber(value) : null);

            entity.Property(e => e.PhoneNumber)
                  .Metadata.SetValueComparer(
                      new ValueComparer<PhoneNumber>(
                          (left, right) => (left == null && right == null) ||
                                           (left != null && right != null && left.Value == right.Value),
                          phone => phone != null ? phone.Value.GetHashCode() : 0,
                          phone => phone != null ? new PhoneNumber(phone.Value) : null));

            entity.Property(e => e.PasswordHash)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.EmailVerified).IsRequired();
            entity.Property(e => e.PhoneVerified).IsRequired();

            // Relationships
            entity.HasOne(e => e.Student)
                  .WithOne()
                  .HasForeignKey<User>(e => e.StudentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Employee)
                  .WithOne()
                  .HasForeignKey<User>(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Relationship: use the PUBLIC navigation
            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Tell EF that RefreshTokens uses the private field "_refreshTokens"
            entity.Navigation(u => u.RefreshTokens)
                  .HasField("_refreshTokens")
                  .UsePropertyAccessMode(PropertyAccessMode.Field);


            // Indexes
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Concurrency
            entity.Property(e => e.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();
        }
    }
}
