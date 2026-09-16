using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAll()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.SalaryHistory)
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<Employee?> GetById(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.SalaryHistory)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Employee>> GetByDepartment(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.DepartmentId == departmentId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task Add(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
            {
                var count = await _context.Employees.CountAsync();
                employee.EmployeeCode = $"EMP{(count + 1):D4}";
            }
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetCurrentSalary(int employeeId)
        {
            var salary = await _context.Salaries
                .Where(s => s.EmployeeId == employeeId && (s.EffectiveTo == null || s.EffectiveTo > DateTime.UtcNow))
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefaultAsync();
            return salary?.NetSalary ?? 0;
        }
    }
}
