using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public record NotificationDto(
    Guid Id,
    string CorrelationId,
    NotificationType Channel,
    string RecipientName,
    string Subject,
    string Body,
    NotificationStatus Status,
    NotificationPriority Priority,
    DateTime CreatedAt,
    DateTime? SentAt,
    string ErrorMessage,
    Dictionary<string, string> Metadata);
}
