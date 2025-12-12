// Domain/ValueObjects/FullName.cs
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class FullName : ValueObject
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullNameString => $"{FirstName} {LastName}";

        private FullName() { }

        public FullName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }
    }
}
