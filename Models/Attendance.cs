namespace BlazorServer.CrudSqlServer.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public TimeOnly? CheckIn { get; set; }
        public TimeOnly? CheckOut { get; set; }
        
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed
        public double? HoursWorked
        {
            get
            {
                if (CheckIn.HasValue && CheckOut.HasValue)
                {
                    return (CheckOut.Value - CheckIn.Value).TotalHours;
                }
                return null;
            }
        }

        public bool IsLate => CheckIn.HasValue && CheckIn.Value > new TimeOnly(8, 30, 0); // Late after 08:30
        public bool IsOvertime => HoursWorked.HasValue && HoursWorked.Value > 8;
    }
}
