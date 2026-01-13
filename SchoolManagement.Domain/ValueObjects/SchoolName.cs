using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.ValueObjects
{
    public class SchoolName : ValueObject
    {
        public string Value { get; private set; }

        private SchoolName(string value) => Value = value;
        public static SchoolName Create(string value) => new SchoolName(value ?? throw new ArgumentNullException(nameof(value)));
        public static SchoolName Empty => new SchoolName("");

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
