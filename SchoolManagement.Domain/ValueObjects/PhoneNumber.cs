// Domain/ValueObjects/PhoneNumber.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SchoolManagement.Domain.ValueObjects
{
    public class PhoneNumber : ValueObject
    {
        public string Value { get; private set; }

        private PhoneNumber() { }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be null or empty", nameof(value));

            var cleaned = Regex.Replace(value, @"[^\d+]", "");
            if (cleaned.Length < 10)
                throw new ArgumentException("Phone number must be at least 10 digits", nameof(value));

            Value = cleaned;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(PhoneNumber phone) => phone?.Value;
        public static explicit operator PhoneNumber(string value) => new PhoneNumber(value);
    }
}
