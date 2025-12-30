using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.ValueObjects
{
    public class Salary : ValueObject
    {
        public decimal BasicSalary { get; }
        public decimal HRA { get; }
        public decimal Allowances { get; }
        public decimal Deductions { get; }

        public decimal GrossSalary => BasicSalary + HRA + Allowances;
        public decimal NetSalary => GrossSalary - Deductions;

        private Salary() { } // EF Core

        public Salary(decimal basicSalary, decimal hra, decimal allowances, decimal deductions)
        {
            if (basicSalary < 0)
                throw new ArgumentException("Basic salary cannot be negative.", nameof(basicSalary));

            if (hra < 0)
                throw new ArgumentException("HRA cannot be negative.", nameof(hra));

            if (allowances < 0)
                throw new ArgumentException("Allowances cannot be negative.", nameof(allowances));

            if (deductions < 0)
                throw new ArgumentException("Deductions cannot be negative.", nameof(deductions));

            BasicSalary = basicSalary;
            HRA = hra;
            Allowances = allowances;
            Deductions = deductions;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return BasicSalary;
            yield return HRA;
            yield return Allowances;
            yield return Deductions;
        }
    }
}
