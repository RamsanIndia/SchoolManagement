using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class NotificationContent : ValueObject
    {
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public Dictionary<string, string> TemplateData { get; private set; }
        public string TemplateId { get; private set; }

        private NotificationContent() { }

        private NotificationContent(
            string subject,
            string body,
            Dictionary<string, string> templateData,
            string templateId)
        {
            Subject = subject;
            Body = body;
            TemplateData = templateData ?? new Dictionary<string, string>();
            TemplateId = templateId;
        }

        public static Result<NotificationContent> Create(
            string subject,
            string body,
            Dictionary<string, string> templateData = null,
            string templateId = null)
        {
            if (string.IsNullOrWhiteSpace(body) && string.IsNullOrWhiteSpace(templateId))
            {
                return Result<NotificationContent>.Failure(
                    "Either body or templateId must be provided");
            }

            return Result<NotificationContent>.Success(
                new NotificationContent(subject, body, templateData, templateId));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Subject ?? string.Empty;
            yield return Body ?? string.Empty;
            yield return TemplateId ?? string.Empty;
        }
    }
}
