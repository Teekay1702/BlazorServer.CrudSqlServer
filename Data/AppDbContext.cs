using Microsoft.EntityFrameworkCore;

namespace BlazorServer.CrudSqlServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Employee> Employees { get; set; }
    }
}
