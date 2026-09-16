using Microsoft.EntityFrameworkCore;
using BlazorServer.CrudSqlServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.HasIndex(u => u.EmployeeId).IsUnique(false);
                entity.HasOne(u => u.Employee)
                    .WithMany()
                    .HasForeignKey(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(u => u.Department)
                    .WithMany()
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Position).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EmployeeCode).HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.ToTable("Employees");

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).HasMaxLength(100).IsRequired();
                entity.Property(d => d.Description).HasMaxLength(500);
                entity.HasIndex(d => d.Name).IsUnique();
                entity.ToTable("Departments");
            });

            modelBuilder.Entity<Salary>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.BasicSalary).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(s => s.Bonus).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Deductions).HasColumnType("decimal(18,2)");
                entity.Property(s => s.PayGrade).HasMaxLength(20);
                entity.Property(s => s.Currency).HasMaxLength(10).IsRequired();
                entity.ToTable("Salaries");

                entity.HasOne(s => s.Employee)
                    .WithMany(e => e.SalaryHistory)
                    .HasForeignKey(s => s.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.EmployeeId);
                entity.HasIndex(s => s.EffectiveFrom);
            });

            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Reason).HasMaxLength(500).IsRequired();
                entity.Property(l => l.ReviewedBy).HasMaxLength(100);
                entity.Property(l => l.ReviewNotes).HasMaxLength(500);
                entity.ToTable("LeaveRequests");

                entity.HasOne(l => l.Employee)
                    .WithMany(e => e.LeaveRequests)
                    .HasForeignKey(l => l.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(l => l.EmployeeId);
                entity.HasIndex(l => l.Status);
                entity.HasIndex(l => l.StartDate);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Notes).HasMaxLength(500);
                entity.ToTable("Attendances");

                entity.HasOne(a => a.Employee)
                    .WithMany(e => e.Attendances)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
                entity.HasIndex(a => a.Date);
                entity.HasIndex(a => a.Status);
            });

            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.BasicSalary).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.Bonus).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Deductions).HasColumnType("decimal(18,2)");
                entity.Property(p => p.OvertimeHours).HasColumnType("decimal(5,2)");
                entity.Property(p => p.OvertimeRate).HasColumnType("decimal(18,2)");
                entity.Property(p => p.PaymentMethod).HasMaxLength(50);
                entity.Property(p => p.Reference).HasMaxLength(100);
                entity.Property(p => p.Notes).HasMaxLength(500);
                entity.ToTable("Payrolls");

                entity.HasOne(p => p.Employee)
                    .WithMany(e => e.Payrolls)
                    .HasForeignKey(p => p.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Salary)
                    .WithMany()
                    .HasForeignKey(p => p.SalaryId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(p => new { p.EmployeeId, p.Year, p.Month }).IsUnique();
                entity.HasIndex(p => p.Status);
            });
        }
    }
}
