using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.ValueObjects
{
    public class BiometricInfo : ValueObject
    {
        public string DeviceId { get; }
        public string TemplateHash { get; }
        public DateTime EnrollmentDate { get; }
        public bool IsActive { get; }

        private BiometricInfo() { } // EF Core

        public BiometricInfo(string deviceId, string templateHash, DateTime? enrollmentDate = null, bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID is required.", nameof(deviceId));

            if (string.IsNullOrWhiteSpace(templateHash))
                throw new ArgumentException("Template hash is required.", nameof(templateHash));

            DeviceId = deviceId;
            TemplateHash = templateHash;
            EnrollmentDate = enrollmentDate ?? DateTime.UtcNow;
            IsActive = isActive;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DeviceId;
            yield return TemplateHash;
            yield return EnrollmentDate;
            yield return IsActive;
        }
    }
}
