using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorServer.CrudSqlServer.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
        
        public int? SalaryId { get; set; }
        public Salary? Salary { get; set; }

        public int Year { get; set; } = DateTime.UtcNow.Year;
        public int Month { get; set; } = DateTime.UtcNow.Month;

        [NotMapped]
        public string Period => $"{Year}-{Month:D2}"; // e.g. 2026-09

        public decimal BasicSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeRate { get; set; } = 150; // ZAR per hour
        public decimal OvertimePay => OvertimeHours * OvertimeRate;
        
        public decimal GrossPay => BasicSalary + Bonus + OvertimePay;
        public decimal NetPay => GrossPay - Deductions;

        public DateTime PayDate { get; set; } = DateTime.UtcNow;
        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
        public string PaymentMethod { get; set; } = "EFT";
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
