using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;
using System.Collections.Generic;
using System.Text.Json;

namespace SchoolManagement.Persistence.Configurations
{
    public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CorrelationId)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Channel)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .IsRequired();

            builder.Property(x => x.Priority)
                   .IsRequired();

            builder.Property(x => x.RetryCount)
                   .IsRequired();

            builder.Property(x => x.MaxRetries)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.ScheduledAt);
            builder.Property(x => x.SentAt);
            builder.Property(x => x.DeliveredAt);

            builder.Property(x => x.ErrorMessage)
                   .HasMaxLength(2000);

            builder.Property(x => x.ExternalId)
                   .HasMaxLength(200);

            // Value Object: Recipient
            builder.OwnsOne(x => x.Recipient, rb =>
            {
                rb.Property(p => p.Email)
                  .HasColumnName("RecipientEmail")
                  .HasMaxLength(256);

                rb.Property(p => p.PhoneNumber)
                  .HasColumnName("RecipientPhoneNumber")
                  .HasMaxLength(30);

                rb.Property(p => p.DeviceToken)
                  .HasColumnName("RecipientDeviceToken")
                  .HasMaxLength(512);

                rb.Property(p => p.Name)
                  .HasColumnName("RecipientName")
                  .HasMaxLength(200);
            });

            builder.Navigation(x => x.Recipient).IsRequired();

            // Value Object: NotificationContent
            builder.OwnsOne(x => x.Content, cb =>
            {
                cb.Property(p => p.Subject)
                  .HasColumnName("ContentSubject")
                  .HasMaxLength(500);

                cb.Property(p => p.Body)
                  .HasColumnName("ContentBody");

                cb.Property(p => p.TemplateId)
                  .HasColumnName("ContentTemplateId")
                  .HasMaxLength(200);

                // TemplateData (Dictionary<string,string>) -> store as JSON
                cb.Property(p => p.TemplateData)
                  .HasColumnName("ContentTemplateData")
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => string.IsNullOrWhiteSpace(v)
                          ? new Dictionary<string, string>()
                          : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
            });

            builder.Navigation(x => x.Content).IsRequired();

            // Metadata (Dictionary<string,string>) -> store as JSON
            builder.Property(x => x.Metadata)
                   .HasColumnName("Metadata")
                   .HasColumnType("jsonb")
                   .HasConversion(
                       v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                       v => string.IsNullOrWhiteSpace(v)
                           ? new Dictionary<string, string>()
                           : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
        }
    }
}
