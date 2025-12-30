using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.ValueObjects
{
    public class SectionCapacity : ValueObject
    {
        public int MaxCapacity { get; private set; }
        public int CurrentStrength { get; private set; }

        private SectionCapacity() { }

        public SectionCapacity(int maxCapacity, int currentStrength = 0)
        {
            if (maxCapacity <= 0)
                throw new ArgumentException("Maximum capacity must be greater than zero.", nameof(maxCapacity));

            if (currentStrength < 0)
                throw new ArgumentException("Current strength cannot be negative.", nameof(currentStrength));

            if (currentStrength > maxCapacity)
                throw new ArgumentException("Current strength cannot exceed maximum capacity.");

            MaxCapacity = maxCapacity;
            CurrentStrength = currentStrength;
        }

        public bool HasAvailableSeats() => CurrentStrength < MaxCapacity;

        public int AvailableSeats() => MaxCapacity - CurrentStrength;

        public SectionCapacity IncrementStrength()
        {
            if (!HasAvailableSeats())
                throw new InvalidOperationException("Section has reached maximum capacity.");

            return new SectionCapacity(MaxCapacity, CurrentStrength + 1);
        }

        public SectionCapacity DecrementStrength()
        {
            if (CurrentStrength <= 0)
                throw new InvalidOperationException("Current strength cannot be negative.");

            return new SectionCapacity(MaxCapacity, CurrentStrength - 1);
        }

        public SectionCapacity UpdateCapacity(int newMaxCapacity)
        {
            if (newMaxCapacity < CurrentStrength)
                throw new ArgumentException("New capacity cannot be less than current strength.");

            return new SectionCapacity(newMaxCapacity, CurrentStrength);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return MaxCapacity;
            yield return CurrentStrength;
        }
    }
}
