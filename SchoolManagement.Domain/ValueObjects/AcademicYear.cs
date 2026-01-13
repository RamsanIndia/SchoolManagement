// ValueObjects/AcademicYear.cs
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class AcademicYear : ValueObject
    {
        public int Year { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        private AcademicYear(int year)
        {
            Year = year;
            StartDate = new DateTime(year, 6, 1);  // June 1st
            EndDate = new DateTime(year + 1, 5, 31);  // May 31st next year
        }

        public static AcademicYear CreateCurrent() => new(DateTime.UtcNow.Year);
        public static AcademicYear Create(int year) => new(year);

        /// ✅ FIXED: Implements abstract method
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Year;
        }
    }
}