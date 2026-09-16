namespace BlazorServer.CrudSqlServer.Models
{
    public class Salary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public decimal BasicSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
        public string PayGrade { get; set; } = string.Empty;
        public string Currency { get; set; } = "ZAR";
        public decimal NetSalary => BasicSalary + Bonus - Deductions;
    }
}
