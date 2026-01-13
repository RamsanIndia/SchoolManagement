// ValueObjects/SchoolTiming.cs
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.ValueObjects
{
    public class SchoolTiming : ValueObject
    {
        public TimeSpan Start { get; }
        public TimeSpan End { get; }
        public TimeSpan Duration => End - Start;

        public SchoolTiming(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }
    }
}
