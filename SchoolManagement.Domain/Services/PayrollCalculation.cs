using System;

namespace SchoolManagement.Domain.Services
{
    public class PayrollCalculation
    {
        public Guid EmployeeId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal Allowances { get; set; }
        public decimal AdditionalAllowances { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal StatutoryDeductions { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal LossOfPayAmount { get; set; }
        public DateTime PayrollMonth { get; set; }

        /// <summary>
        /// Gets a formatted summary of the payroll calculation
        /// </summary>
        public string GetSummary()
        {
            return $@"
                Payroll Summary - {PayrollMonth:MMMM yyyy}
                =====================================
                Basic Salary:           {BasicSalary:C}
                HRA:                    {HRA:C}
                Allowances:             {Allowances:C}
                Additional Allowances:  {AdditionalAllowances:C}
                ─────────────────────
                Gross Salary:           {GrossSalary:C}
                
                Loss of Pay:            {LossOfPayAmount:C}
                Adjusted Gross:         {GrossSalary - LossOfPayAmount:C}
                
                Statutory Deductions:   {StatutoryDeductions:C}
                Other Deductions:       {OtherDeductions:C}
                Total Deductions:       {TotalDeductions:C}
                ─────────────────────
                Net Salary:             {NetSalary:C}
                
                Attendance: {PresentDays}/{WorkingDays} days (Absent: {AbsentDays} days)
            ";
        }
    }
}
