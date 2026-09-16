using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Services
{
    public class SalaryService
    {
        private readonly AppDbContext _context;

        public SalaryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Salary>> GetAll()
        {
            return await _context.Salaries
                .Include(s => s.Employee)
                .ThenInclude(e => e.Department)
                .AsNoTracking()
                .OrderByDescending(s => s.EffectiveFrom)
                .ToListAsync();
        }

        public async Task<List<Salary>> GetByEmployee(int employeeId)
        {
            return await _context.Salaries
                .Include(s => s.Employee)
                .Where(s => s.EmployeeId == employeeId)
                .AsNoTracking()
                .OrderByDescending(s => s.EffectiveFrom)
                .ToListAsync();
        }

        public async Task<Salary?> GetCurrentSalary(int employeeId)
        {
            return await _context.Salaries
                .Include(s => s.Employee)
                .Where(s => s.EmployeeId == employeeId && (s.EffectiveTo == null || s.EffectiveTo > DateTime.UtcNow))
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<Salary?> GetById(int id)
        {
            return await _context.Salaries
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task Add(Salary salary)
        {
            // Close previous active salary if exists
            var current = await _context.Salaries
                .Where(s => s.EmployeeId == salary.EmployeeId && s.EffectiveTo == null)
                .ToListAsync();

            foreach (var prev in current)
            {
                prev.EffectiveTo = salary.EffectiveFrom.AddDays(-1);
            }

            _context.Salaries.Add(salary);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Salary salary)
        {
            var existingSalary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.Id == salary.Id);

            if (existingSalary == null)
            {
                return;
            }

            existingSalary.EmployeeId = salary.EmployeeId;
            existingSalary.BasicSalary = salary.BasicSalary;
            existingSalary.Bonus = salary.Bonus;
            existingSalary.Deductions = salary.Deductions;
            existingSalary.EffectiveFrom = salary.EffectiveFrom;
            existingSalary.EffectiveTo = salary.EffectiveTo;
            existingSalary.PayGrade = salary.PayGrade;
            existingSalary.Currency = salary.Currency;

            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary != null)
            {
                _context.Salaries.Remove(salary);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalPayroll()
        {
            var activeSalaries = await _context.Salaries
                .Where(s => s.EffectiveTo == null || s.EffectiveTo > DateTime.UtcNow)
                .GroupBy(s => s.EmployeeId)
                .Select(g => g.OrderByDescending(s => s.EffectiveFrom).First())
                .ToListAsync();
            
            return activeSalaries.Sum(s => s.NetSalary);
        }

        public async Task<Dictionary<string, decimal>> GetPayrollByDepartment()
        {
            var salaries = await _context.Salaries
                .Include(s => s.Employee)
                .ThenInclude(e => e.Department)
                .Where(s => s.EffectiveTo == null || s.EffectiveTo > DateTime.UtcNow)
                .ToListAsync();

            var latestByEmployee = salaries
                .GroupBy(s => s.EmployeeId)
                .Select(g => g.OrderByDescending(s => s.EffectiveFrom).First())
                .GroupBy(s => s.Employee.Department?.Name ?? "Unassigned")
                .ToDictionary(g => g.Key, g => g.Sum(s => s.NetSalary));

            return latestByEmployee;
        }
    }
}
