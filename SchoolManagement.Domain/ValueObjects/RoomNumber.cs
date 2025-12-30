using SchoolManagement.Domain.Exceptions;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class RoomNumber : ValueObject
    {
        private const int MaxLength = 20;
        private const int MinLength = 1;

        public string Value { get; private set; }

        private RoomNumber() { }

        public RoomNumber(string value)
        {
            ValidateRoomNumber(value);
            Value = value.Trim().ToUpperInvariant();
        }

        public static RoomNumber Create(string value)
        {
            return new RoomNumber(value);
        }

        private static void ValidateRoomNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidRoomNumberException(
                    value ?? "null",
                    "Room number cannot be empty or whitespace");

            var trimmedValue = value.Trim();

            if (trimmedValue.Length < MinLength)
                throw new InvalidRoomNumberException(
                    value,
                    $"Room number must be at least {MinLength} character");

            if (trimmedValue.Length > MaxLength)
                throw new InvalidRoomNumberException(
                    value,
                    $"Room number cannot exceed {MaxLength} characters");
        }

        public static implicit operator string(RoomNumber roomNumber) => roomNumber?.Value;
        public static explicit operator RoomNumber(string value) => new RoomNumber(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
