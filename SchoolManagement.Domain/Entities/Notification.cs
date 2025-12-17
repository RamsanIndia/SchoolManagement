using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.Entities
{
    public class Notification : BaseEntity, IAggregateRoot
    {
        public string CorrelationId { get; private set; }
        public NotificationType Channel { get; private set; }
        public Recipient Recipient { get; private set; }
        public NotificationContent Content { get; private set; }
        public NotificationStatus Status { get; private set; }
        public NotificationPriority Priority { get; private set; }
        public int RetryCount { get; private set; }
        public int MaxRetries { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ScheduledAt { get; private set; }
        public DateTime? SentAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public string ErrorMessage { get; private set; }
        public string ExternalId { get; private set; }
        public Dictionary<string, string> Metadata { get; private set; }

        private Notification() { }

        private Notification(
            Guid id,
            string correlationId,
            NotificationType channel,
            Recipient recipient,
            NotificationContent content,
            NotificationPriority priority,
            DateTime? scheduledAt,
            Dictionary<string, string> metadata)
        {
            Id = id;
            CorrelationId = correlationId;
            Channel = channel;
            Recipient = recipient;
            Content = content;
            Priority = priority;
            Status = NotificationStatus.Pending;
            RetryCount = 0;
            MaxRetries = 3;
            CreatedAt = DateTime.UtcNow;
            ScheduledAt = scheduledAt;
            Metadata = metadata ?? new Dictionary<string, string>();

            AddDomainEvent(new NotificationCreatedEvent(Id, Channel, Priority, CreatedAt));
        }

        public static Result<Notification> Create(
            string correlationId,
            NotificationType channel,
            Recipient recipient,
            NotificationContent content,
            NotificationPriority priority = NotificationPriority.Normal,
            DateTime? scheduledAt = null,
            Dictionary<string, string> metadata = null)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return Result<Notification>.Failure("CorrelationId is required");
            }

            if (recipient == null)
            {
                return Result<Notification>.Failure("Recipient is required");
            }

            if (content == null)
            {
                return Result<Notification>.Failure("Content is required");
            }

            var notification = new Notification(
                Guid.NewGuid(),
                correlationId,
                channel,
                recipient,
                content,
                priority,
                scheduledAt,
                metadata);

            return Result<Notification>.Success(notification);
        }

        public Result MarkAsProcessing()
        {
            if (Status != NotificationStatus.Pending)
            {
                return Result.Failure($"Cannot process notification in {Status} status");
            }

            Status = NotificationStatus.Processing;
            AddDomainEvent(new NotificationProcessingEvent(Id, DateTime.UtcNow));
            return Result.Success();
        }

        public Result MarkAsSent(string externalId)
        {
            if (Status != NotificationStatus.Processing)
            {
                return Result.Failure($"Cannot mark as sent from {Status} status");
            }

            Status = NotificationStatus.Sent;
            SentAt = DateTime.UtcNow;
            ExternalId = externalId;
            ErrorMessage = null;

            AddDomainEvent(new NotificationSentEvent(Id, Channel, SentAt.Value, externalId));
            return Result.Success();
        }

        public Result MarkAsDelivered()
        {
            if (Status != NotificationStatus.Sent)
            {
                return Result.Failure($"Cannot mark as delivered from {Status} status");
            }

            Status = NotificationStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;

            AddDomainEvent(new NotificationDeliveredEvent(Id, Channel, DeliveredAt.Value));
            return Result.Success();
        }

        public Result MarkAsFailed(string errorMessage)
        {
            if (Status == NotificationStatus.Delivered || Status == NotificationStatus.Cancelled)
            {
                return Result.Failure($"Cannot mark as failed from {Status} status");
            }

            Status = NotificationStatus.Failed;
            ErrorMessage = errorMessage;
            RetryCount++;

            AddDomainEvent(new NotificationFailedEvent(
                Id, Channel, DateTime.UtcNow, errorMessage, RetryCount, MaxRetries));

            return Result.Success();
        }

        public Result Retry()
        {
            if (Status != NotificationStatus.Failed)
            {
                return Result.Failure("Only failed notifications can be retried");
            }

            if (RetryCount >= MaxRetries)
            {
                return Result.Failure($"Maximum retry attempts ({MaxRetries}) exceeded");
            }

            Status = NotificationStatus.Pending;
            ErrorMessage = null;

            AddDomainEvent(new NotificationRetryEvent(Id, RetryCount, DateTime.UtcNow));
            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status == NotificationStatus.Sent ||
                Status == NotificationStatus.Delivered ||
                Status == NotificationStatus.Cancelled)
            {
                return Result.Failure($"Cannot cancel notification in {Status} status");
            }

            Status = NotificationStatus.Cancelled;
            AddDomainEvent(new NotificationCancelledEvent(Id, DateTime.UtcNow));
            return Result.Success();
        }

        public bool CanRetry()
        {
            return Status == NotificationStatus.Failed && RetryCount < MaxRetries;
        }

        public bool IsReadyToSend()
        {
            if (Status != NotificationStatus.Pending)
                return false;

            if (!ScheduledAt.HasValue)
                return true;

            return ScheduledAt.Value <= DateTime.UtcNow;
        }
    }
}
