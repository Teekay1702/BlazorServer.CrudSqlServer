using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Services
{
    public class PayrollService
    {
        private readonly AppDbContext _context;
        public PayrollService(AppDbContext context) => _context = context;

        public async Task<List<Payroll>> GetAll()
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .Include(p => p.Salary)
                .AsNoTracking()
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ThenBy(p => p.Employee.Name)
                .ToListAsync();
        }

        public async Task<List<Payroll>> GetByPeriod(int year, int month)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .Where(p => p.Year == year && p.Month == month)
                .AsNoTracking()
                .OrderBy(p => p.Employee.Name)
                .ToListAsync();
        }

        public async Task<List<Payroll>> GetByEmployee(int employeeId)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .AsNoTracking()
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();
        }

        public async Task<Payroll?> GetById(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .Include(p => p.Salary)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payroll> GenerateForEmployee(int employeeId, int year, int month, decimal overtimeHours = 0)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var currentSalary = await _context.Salaries
                .Where(s => s.EmployeeId == employeeId && (s.EffectiveTo == null || s.EffectiveTo > new DateTime(year, month, 1)))
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefaultAsync();

            if (currentSalary == null)
                throw new InvalidOperationException($"No active salary found for {employee.Name}. Add salary first.");

            var existing = await _context.Payrolls
                .AnyAsync(p => p.EmployeeId == employeeId && p.Year == year && p.Month == month);

            if (existing)
                throw new InvalidOperationException($"Payroll already exists for {employee.Name} for {year}-{month:D2}");

            var payroll = new Payroll
            {
                EmployeeId = employeeId,
                SalaryId = currentSalary.Id,
                Year = year,
                Month = month,
                BasicSalary = currentSalary.BasicSalary,
                Bonus = currentSalary.Bonus,
                Deductions = currentSalary.Deductions,
                OvertimeHours = overtimeHours,
                OvertimeRate = 150,
                PayDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                Status = PayrollStatus.Draft,
                PaymentMethod = "EFT",
                Reference = $"PAY-{year}{month:D2}-{employee.EmployeeCode}",
                Notes = $"Auto-generated from salary {currentSalary.PayGrade}"
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();
            return payroll;
        }

        public async Task<List<Payroll>> GenerateForAll(int year, int month)
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Include(e => e.SalaryHistory)
                .ToListAsync();

            var results = new List<Payroll>();
            foreach (var emp in employees)
            {
                try
                {
                    var payroll = await GenerateForEmployee(emp.Id, year, month);
                    results.Add(payroll);
                }
                catch
                {
                    // Skip employees without salary
                }
            }
            return results;
        }

        public async Task Update(Payroll payroll)
        {
            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();
        }

        public async Task Process(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                payroll.Status = PayrollStatus.Processed;
                payroll.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsPaid(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                payroll.Status = PayrollStatus.Paid;
                payroll.PayDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Delete(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalForPeriod(int year, int month)
        {
            return await _context.Payrolls
                .Where(p => p.Year == year && p.Month == month && p.Status != PayrollStatus.Cancelled)
                .SumAsync(p => p.BasicSalary + p.Bonus + (p.OvertimeHours * p.OvertimeRate) - p.Deductions);
        }

        public async Task<Dictionary<string, decimal>> GetByDepartmentForPeriod(int year, int month)
        {
            var payrolls = await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .Where(p => p.Year == year && p.Month == month && p.Status != PayrollStatus.Cancelled)
                .ToListAsync();

            return payrolls.GroupBy(p => p.Employee.Department?.Name ?? "Unassigned")
                .ToDictionary(g => g.Key, g => g.Sum(p => p.NetPay));
        }
    }
}
