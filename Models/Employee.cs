namespace BlazorServer.CrudSqlServer.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        // HR fields
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string EmployeeCode { get; set; } = string.Empty; 

        // Department FK
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Navigation
        public ICollection<Salary> SalaryHistory { get; set; } = new List<Salary>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    }
}
