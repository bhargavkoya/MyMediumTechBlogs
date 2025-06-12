using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionsAndLinqInDotNet
{
    public class EmployeeManagementSystem
    {
        private readonly List<Employee> _employees;

        public EmployeeManagementSystem()
        {
            _employees = new List<Employee>();
        }

        //search employees with multiple criteria
        public IEnumerable<Employee> SearchEmployees(string department = null,
            decimal? minSalary = null, int? minExperience = null)
        {
            var query = _employees.AsQueryable();

            if (!string.IsNullOrEmpty(department))
                query = query.Where(e => e.Department == department);

            if (minSalary.HasValue)
                query = query.Where(e => e.Salary >= minSalary.Value);

            if (minExperience.HasValue)
                query = query.Where(e => e.Experience >= minExperience.Value);

            return query.OrderBy(e => e.Name);
        }

        //get department statistics
        public IEnumerable<DepartmentStats> GetDepartmentStatistics()
        {
            return _employees
                .GroupBy(e => e.Department)
                .Select(g => new DepartmentStats
                {
                    Department = g.Key,
                    EmployeeCount = g.Count(),
                    AverageSalary = g.Average(e => e.Salary),
                    MinSalary = g.Min(e => e.Salary),
                    MaxSalary = g.Max(e => e.Salary),
                    TotalExperience = g.Sum(e => e.Experience)
                })
                .OrderByDescending(ds => ds.AverageSalary);
        }

        //get paginated employee list
        public PagedResult<Employee> GetEmployeesPaged(int page, int pageSize)
        {
            var totalCount = _employees.Count;
            var employees = _employees
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Employee>
            {
                Items = employees,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        //analyze skill distribution
        public IEnumerable<SkillAnalysis> AnalyzeSkills()
        {
            return _employees
                .SelectMany(e => e.Skills.Select(skill => new { Employee = e, Skill = skill }))
                .GroupBy(x => x.Skill)
                .Select(g => new SkillAnalysis
                {
                    Skill = g.Key,
                    EmployeeCount = g.Count(),
                    AverageSalaryForSkill = g.Average(x => x.Employee.Salary),
                    Departments = g.Select(x => x.Employee.Department).Distinct().ToList()
                })
                .OrderByDescending(sa => sa.EmployeeCount);
        }
    }

    //supporting classes
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; }
        public int Experience { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class DepartmentStats
    {
        public string Department { get; set; }
        public int EmployeeCount { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public int TotalExperience { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SkillAnalysis
    {
        public string Skill { get; set; }
        public int EmployeeCount { get; set; }
        public decimal AverageSalaryForSkill { get; set; }
        public List<string> Departments { get; set; }
    }
}
