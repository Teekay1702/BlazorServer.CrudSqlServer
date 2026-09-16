using Microsoft.AspNetCore.Identity;
using BlazorServer.CrudSqlServer.Models;

namespace BlazorServer.CrudSqlServer.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Roles for HR system
            string[] roles = { "Admin", "HRManager", "Manager", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Default Admin user
            var adminEmail = "admin@hrsystem.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "HRManager");
                }
            }

            // Default HR Manager
            var hrEmail = "hr@hrsystem.local";
            var hrUser = await userManager.FindByEmailAsync(hrEmail);
            if (hrUser == null)
            {
                hrUser = new User
                {
                    UserName = hrEmail,
                    Email = hrEmail,
                    FullName = "HR Manager",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(hrUser, "Hr@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(hrUser, "HRManager");
                }
            }
        }
    }
}
