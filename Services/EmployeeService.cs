using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService (AppDbContext context)
        {
            _context = context;
        }

        public async Task <List<Employee>> GetAll()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task Add(Employee employee)
        {
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
    }
}
