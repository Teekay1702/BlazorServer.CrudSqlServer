using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Services
{
    public class AttendanceService
    {
        private readonly AppDbContext _context;
        public AttendanceService(AppDbContext context) => _context = context;

        public async Task<List<Attendance>> GetAll()
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
                .AsNoTracking()
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.Name)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByEmployee(int employeeId)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .AsNoTracking()
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByDateRange(DateTime start, DateTime end)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
                .Where(a => a.Date >= start && a.Date <= end)
                .AsNoTracking()
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetToday()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
                .Where(a => a.Date == today)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Attendance?> GetById(int id)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task Add(Attendance attendance)
        {
            // Check duplicate
            var exists = await _context.Attendances
                .AnyAsync(a => a.EmployeeId == attendance.EmployeeId && a.Date == attendance.Date.Date);
            
            if (exists)
                throw new InvalidOperationException($"Attendance already recorded for this employee on {attendance.Date:yyyy-MM-dd}");

            attendance.Date = attendance.Date.Date;
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddForDate(DateTime date, List<int> employeeIds, AttendanceStatus status = AttendanceStatus.Present)
        {
            date = date.Date;
            var existingIds = await _context.Attendances
                .Where(a => a.Date == date)
                .Select(a => a.EmployeeId)
                .ToListAsync();

            var newRecords = employeeIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new Attendance
                {
                    EmployeeId = id,
                    Date = date,
                    Status = status,
                    CheckIn = status == AttendanceStatus.Present ? new TimeOnly(8, 0) : null,
                    CheckOut = status == AttendanceStatus.Present ? new TimeOnly(17, 0) : null
                });

            _context.Attendances.AddRange(newRecords);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var att = await _context.Attendances.FindAsync(id);
            if (att != null)
            {
                _context.Attendances.Remove(att);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CheckIn(int employeeId, TimeOnly? time = null)
        {
            var today = DateTime.UtcNow.Date;
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = employeeId,
                    Date = today,
                    CheckIn = time ?? TimeOnly.FromDateTime(DateTime.Now),
                    Status = AttendanceStatus.Present
                };
                _context.Attendances.Add(attendance);
            }
            else
            {
                attendance.CheckIn = time ?? TimeOnly.FromDateTime(DateTime.Now);
                if (attendance.CheckIn > new TimeOnly(8, 30))
                    attendance.Status = AttendanceStatus.Late;
            }
            await _context.SaveChangesAsync();
        }

        public async Task CheckOut(int employeeId, TimeOnly? time = null)
        {
            var today = DateTime.UtcNow.Date;
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (attendance != null)
            {
                attendance.CheckOut = time ?? TimeOnly.FromDateTime(DateTime.Now);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<AttendanceStatus, int>> GetStatsForMonth(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            
            var data = await _context.Attendances
                .Where(a => a.Date >= start && a.Date <= end)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return data.ToDictionary(x => x.Status, x => x.Count);
        }
    }
}
