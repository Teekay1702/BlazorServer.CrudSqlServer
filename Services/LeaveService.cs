using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Services
{
    public class LeaveService
    {
        private readonly AppDbContext _context;
        public LeaveService(AppDbContext context) => _context = context;

        public async Task<List<LeaveRequest>> GetAll()
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
                .AsNoTracking()
                .OrderByDescending(l => l.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetByEmployee(int employeeId)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Where(l => l.EmployeeId == employeeId)
                .AsNoTracking()
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetPending()
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
                .Where(l => l.Status == LeaveStatus.Pending)
                .AsNoTracking()
                .OrderBy(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetById(int id)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task Add(LeaveRequest leave)
        {
            if (leave.EndDate < leave.StartDate)
                throw new InvalidOperationException("End date cannot be before start date");

            // Check overlapping leaves
            var overlapping = await _context.LeaveRequests
                .AnyAsync(l => l.EmployeeId == leave.EmployeeId 
                    && l.Status != LeaveStatus.Rejected 
                    && l.Status != LeaveStatus.Cancelled
                    && l.StartDate <= leave.EndDate 
                    && l.EndDate >= leave.StartDate);

            if (overlapping)
                throw new InvalidOperationException("Employee already has leave in this period");

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();
        }

        public async Task Update(LeaveRequest leave)
        {
            _context.LeaveRequests.Update(leave);
            await _context.SaveChangesAsync();
        }

        public async Task Approve(int id, string reviewedBy, string notes = "")
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                leave.Status = LeaveStatus.Approved;
                leave.ReviewedBy = reviewedBy;
                leave.ReviewNotes = notes;
                leave.ReviewedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Reject(int id, string reviewedBy, string notes)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                leave.Status = LeaveStatus.Rejected;
                leave.ReviewedBy = reviewedBy;
                leave.ReviewNotes = notes;
                leave.ReviewedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Delete(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                _context.LeaveRequests.Remove(leave);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<LeaveType, int>> GetLeaveBalance(int employeeId, int year)
        {
            var used = await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Approved && l.StartDate.Year == year)
                .ToListAsync();

            return used.GroupBy(l => l.LeaveType)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.TotalDays));
        }
    }
}
