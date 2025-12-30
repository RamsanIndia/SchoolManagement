using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.Services
{
    public interface ISalaryCalculationService
    {
        /// <summary>
        /// Calculates gross salary from base salary and additional allowances
        /// </summary>
        decimal CalculateGrossSalary(Salary baseSalary, IEnumerable<Allowance> allowances = null);

        /// <summary>
        /// Calculates net salary from gross salary and deductions
        /// </summary>
        decimal CalculateNetSalary(decimal grossSalary, IEnumerable<Deduction> deductions = null);

        /// <summary>
        /// Processes complete monthly payroll for an employee
        /// </summary>
        PayrollCalculation ProcessPayroll(
            Employee employee,
            IEnumerable<EmployeeAttendance> attendances,
            DateTime payrollMonth,
            IEnumerable<Allowance> allowances = null,
            IEnumerable<Deduction> deductions = null);

        /// <summary>
        /// Calculates PF separately
        /// </summary>
        decimal CalculatePF(decimal grossSalary);

        /// <summary>
        /// Calculates ESI separately
        /// </summary>
        decimal CalculateESI(decimal grossSalary);

        /// <summary>
        /// Calculates income tax on net salary
        /// </summary>
        decimal CalculateIncomeTax(decimal netSalary);
    }
}
