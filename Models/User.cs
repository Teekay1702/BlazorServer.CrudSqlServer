using Microsoft.AspNetCore.Identity;

namespace BlazorServer.CrudSqlServer.Models
{
    public class User : IdentityUser
    {
        // Extended profile for HR system
        public string FullName { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? ProfilePictureUrl { get; set; }
    }
}
