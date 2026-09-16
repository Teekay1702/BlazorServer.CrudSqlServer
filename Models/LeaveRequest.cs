namespace BlazorServer.CrudSqlServer.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public LeaveType LeaveType { get; set; } = LeaveType.Annual;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHalfDay { get; set; } = false;
        
        public string Reason { get; set; } = string.Empty;
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
        public string ReviewNotes { get; set; } = string.Empty;

        // Computed
        public int TotalDays
        {
            get
            {
                if (IsHalfDay) return 1;
                var days = (EndDate.Date - StartDate.Date).Days + 1;
                return days > 0 ? days : 1;
            }
        }
    }
}
