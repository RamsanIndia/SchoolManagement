using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.ValueObjects
{
    public class SchoolCode : ValueObject
    {
        public string Value { get; private set; }

        private SchoolCode(string value) => Value = value;
        public static SchoolCode Create(string value)
        {
            if (value?.Length != 6 || !value.All(char.IsUpper))
                throw new ArgumentException("Invalid school code format");
            return new SchoolCode(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
