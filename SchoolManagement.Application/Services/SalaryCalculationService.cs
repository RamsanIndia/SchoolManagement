using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Services;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Application.Services
{
    public class SalaryCalculationService : ISalaryCalculationService
    {
        /// <summary>
        /// Calculates gross salary from base salary and additional allowances
        /// Salary VO has: BasicSalary, HRA, Allowances, Deductions
        /// </summary>
        public decimal CalculateGrossSalary(Salary baseSalary, IEnumerable<Allowance> allowances = null)
        {
            if (baseSalary == null)
                throw new ArgumentNullException(nameof(baseSalary));

            var totalAdditionalAllowances = 0m;
            if (allowances != null)
            {
                totalAdditionalAllowances = allowances
                    .Where(a => a.IsActive)
                    .Sum(a => a.IsPercentage
                        ? (baseSalary.BasicSalary * a.Amount / 100)
                        : a.Amount);
            }

            // Gross Salary = BasicSalary + HRA + Allowances + Additional Allowances
            return baseSalary.BasicSalary + baseSalary.HRA + baseSalary.Allowances + totalAdditionalAllowances;
        }

        /// <summary>
        /// Calculates net salary from gross salary and deductions
        /// </summary>
        public decimal CalculateNetSalary(decimal grossSalary, IEnumerable<Deduction> deductions = null)
        {
            if (grossSalary < 0)
                throw new ArgumentException("Gross salary cannot be negative.", nameof(grossSalary));

            var totalDeductions = 0m;
            if (deductions != null)
            {
                totalDeductions = deductions
                    .Where(d => d.IsActive)
                    .Sum(d => d.IsPercentage
                        ? (grossSalary * d.Amount / 100)
                        : d.Amount);
            }

            return grossSalary - totalDeductions;
        }

        /// <summary>
        /// Processes complete monthly payroll for an employee with all allowances and deductions
        /// </summary>
        public PayrollCalculation ProcessPayroll(
            Employee employee,
            IEnumerable<EmployeeAttendance> attendances,
            DateTime payrollMonth,
            IEnumerable<Allowance> allowances = null,
            IEnumerable<Deduction> deductions = null)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.SalaryInfo == null)
                throw new InvalidOperationException($"Employee {employee.EmployeeId} has no salary information.");

            if (attendances == null)
                attendances = new List<EmployeeAttendance>();

            // Get working days and present days
            var workingDays = GetWorkingDaysInMonth(payrollMonth);
            var presentDays = attendances
                .Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            var absentDays = workingDays - presentDays;

            // Calculate gross salary
            var baseSalaryAmount = employee.SalaryInfo.BasicSalary +
                                  employee.SalaryInfo.HRA +
                                  employee.SalaryInfo.Allowances;

            var totalAdditionalAllowances = allowances?.Where(a => a.IsActive)
                .Sum(a => a.IsPercentage
                    ? (employee.SalaryInfo.BasicSalary * a.Amount / 100)
                    : a.Amount) ?? 0;

            var grossSalary = baseSalaryAmount + totalAdditionalAllowances;

            // Calculate loss of pay
            var dailyRate = employee.SalaryInfo.BasicSalary / workingDays;
            var lossOfPayAmount = absentDays * dailyRate;

            // Adjusted gross salary after loss of pay
            var adjustedGrossSalary = grossSalary - lossOfPayAmount;

            // Calculate all deductions
            var statutoryDeductions = CalculateStatutoryDeductions(adjustedGrossSalary);
            var otherDeductions = deductions?.Where(d => d.IsActive)
                .Sum(d => d.IsPercentage
                    ? (adjustedGrossSalary * d.Amount / 100)
                    : d.Amount) ?? 0;

            var totalDeductions = statutoryDeductions + otherDeductions;

            // Calculate net salary
            var netSalary = adjustedGrossSalary - totalDeductions;

            return new PayrollCalculation
            {
                EmployeeId = employee.Id,
                BasicSalary = employee.SalaryInfo.BasicSalary,
                HRA = employee.SalaryInfo.HRA,
                Allowances = employee.SalaryInfo.Allowances,
                AdditionalAllowances = totalAdditionalAllowances,
                GrossSalary = adjustedGrossSalary,
                StatutoryDeductions = statutoryDeductions,
                OtherDeductions = otherDeductions,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary,
                WorkingDays = workingDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LossOfPayAmount = lossOfPayAmount,
                PayrollMonth = payrollMonth
            };
        }

        /// <summary>
        /// Processes payroll with only required parameters (simple version)
        /// </summary>
        public PayrollCalculation ProcessPayrollSimple(
            Employee employee,
            IEnumerable<EmployeeAttendance> attendances,
            DateTime payrollMonth)
        {
            return ProcessPayroll(employee, attendances, payrollMonth, null, null);
        }

        /// <summary>
        /// Gets the number of working days in a month (excluding weekends)
        /// </summary>
        private int GetWorkingDaysInMonth(DateTime month)
        {
            if (month == default)
                throw new ArgumentException("Invalid month.", nameof(month));

            var firstDay = new DateTime(month.Year, month.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var workingDays = 0;

            for (var day = firstDay; day <= lastDay; day = day.AddDays(1))
            {
                if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }

            return workingDays;
        }

        /// <summary>
        /// Calculates total statutory deductions (PF, ESI, PT)
        /// </summary>
        private decimal CalculateStatutoryDeductions(decimal grossSalary)
        {
            if (grossSalary < 0)
                throw new ArgumentException("Gross salary cannot be negative.", nameof(grossSalary));

            var pfDeduction = CalculatePF(grossSalary);
            var esiDeduction = CalculateESI(grossSalary);
            var professionalTax = CalculateProfessionalTax(grossSalary);

            return pfDeduction + esiDeduction + professionalTax;
        }

        /// <summary>
        /// Calculates Provident Fund (PF) deduction - 12% of gross salary
        /// </summary>
        public decimal CalculatePF(decimal grossSalary)
        {
            if (grossSalary < 0)
                throw new ArgumentException("Gross salary cannot be negative.", nameof(grossSalary));

            return grossSalary * 0.12m;
        }

        /// <summary>
        /// Calculates Employee State Insurance (ESI) - 1.75% if salary <= 21,000
        /// </summary>
        public decimal CalculateESI(decimal grossSalary)
        {
            if (grossSalary < 0)
                throw new ArgumentException("Gross salary cannot be negative.", nameof(grossSalary));

            return grossSalary <= 21000 ? grossSalary * 0.0175m : 0;
        }

        /// <summary>
        /// Calculates Professional Tax (PT) - Fixed or percentage based on salary
        /// Note: PT rates vary by state in India
        /// </summary>
        private decimal CalculateProfessionalTax(decimal grossSalary)
        {
            if (grossSalary < 0)
                throw new ArgumentException("Gross salary cannot be negative.", nameof(grossSalary));

            // Example: Fixed PT amount (adjust based on state regulations)
            // Common slabs: 0-10k (0), 10k-20k (100), 20k-30k (150), 30k+ (200)
            if (grossSalary < 10000)
                return 0;
            else if (grossSalary < 20000)
                return 100;
            else if (grossSalary < 30000)
                return 150;
            else
                return 200;
        }

        /// <summary>
        /// Calculates income tax on net salary (simplified calculation)
        /// Note: This is a basic calculation. Real implementation should use actual tax slabs
        /// </summary>
        public decimal CalculateIncomeTax(decimal annualNetSalary)
        {
            if (annualNetSalary < 0)
                throw new ArgumentException("Annual salary cannot be negative.", nameof(annualNetSalary));

            // Simplified slab system (2024-2025)
            // Note: These are basic slabs, adjust based on actual tax regulations

            if (annualNetSalary <= 300000)
                return 0; // No tax

            else if (annualNetSalary <= 700000)
                return (annualNetSalary - 300000) * 0.05m; // 5% on amount above 3L

            else if (annualNetSalary <= 1000000)
                return ((annualNetSalary - 700000) * 0.20m) + ((700000 - 300000) * 0.05m); // 20% on amount between 7L-10L

            else if (annualNetSalary <= 1200000)
                return ((annualNetSalary - 1000000) * 0.30m) + ((1000000 - 700000) * 0.20m) + ((700000 - 300000) * 0.05m); // 30% on amount above 10L

            else
                return ((annualNetSalary - 1200000) * 0.30m) + ((1200000 - 1000000) * 0.30m) + ((1000000 - 700000) * 0.20m) + ((700000 - 300000) * 0.05m);
        }

        /// <summary>
        /// Calculates Take-Home Salary (after all deductions including income tax)
        /// </summary>
        public decimal CalculateTakeHomeSalary(decimal netSalary, decimal monthlyIncomeTax)
        {
            if (netSalary < 0)
                throw new ArgumentException("Net salary cannot be negative.", nameof(netSalary));

            if (monthlyIncomeTax < 0)
                throw new ArgumentException("Income tax cannot be negative.", nameof(monthlyIncomeTax));

            return netSalary - monthlyIncomeTax;
        }
    }
}
