using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.SOLID
{
    //GOOD: Separated responsibilities
    //This class handles multiple concerns: employee data, payroll calculation, database operations, and report generation. 
    //A better approach separates these responsibilities.
    public class Employee
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
    }

    public class PayrollCalculator
    {
        public static decimal CalculateAnnualPay(Employee employee)
        {
            return employee.Salary * 12;
        }
    }

    public class EmployeeRepository
    {
        public static void Save(Employee employee)
        {
            Console.WriteLine($"Saving {employee.Name} to database");
        }
    }

    public class ReportGeneratorSRP
    {
        public static string GenerateEmployeeReport(Employee employee)
        {
            return $"Employee Report: {employee.Name}";
        }
    }
}
