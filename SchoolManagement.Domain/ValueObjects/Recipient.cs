using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class Recipient : ValueObject
    {
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string DeviceToken { get; private set; }
        public string Name { get; private set; }

        private Recipient() { }

        private Recipient(string email, string phoneNumber, string deviceToken, string name)
        {
            Email = email;
            PhoneNumber = phoneNumber;
            DeviceToken = deviceToken;
            Name = name;
        }

        public static Result<Recipient> Create(string email, string phoneNumber, string deviceToken, string name)
        {
            if (string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(phoneNumber) &&
                string.IsNullOrWhiteSpace(deviceToken))
            {
                return Result<Recipient>.Failure("At least one contact method must be provided");
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                return Result<Recipient>.Failure("Invalid email format");
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !IsValidPhoneNumber(phoneNumber))
            {
                return Result<Recipient>.Failure("Invalid phone number format");
            }

            return Result<Recipient>.Success(new Recipient(email, phoneNumber, deviceToken, name));
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(
                phoneNumber, @"^\+?[1-9]\d{1,14}$");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Email ?? string.Empty;
            yield return PhoneNumber ?? string.Empty;
            yield return DeviceToken ?? string.Empty;
        }
    }
}
