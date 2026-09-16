using BlazorServer.CrudSqlServer.Components;
using BlazorServer.CrudSqlServer.Data;
using BlazorServer.CrudSqlServer.Models;
using BlazorServer.CrudSqlServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Relaxed for dev - tighten for production
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// AuthZ
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("HRManager", policy => policy.RequireRole("Admin", "HRManager"));
    options.AddPolicy("Manager", policy => policy.RequireRole("Admin", "HRManager", "Manager"));
});
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<SalaryService>();
builder.Services.AddScoped<LeaveService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<PayrollService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
try
{
    await SeedData.Initialize(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error seeding identity data");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/account/login", async (
    HttpContext httpContext,
    UserManager<User> userManager,
    SignInManager<User> signInManager) =>
{
    var form = await httpContext.Request.ReadFormAsync();

    var email = form["Email"].ToString();
    var password = form["Password"].ToString();
    var rememberMe = form["RememberMe"] == "true";

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
    {
        return Results.Redirect("/login?error=invalid");
    }

    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        return Results.Redirect("/login?error=invalid");
    }

    var result = await signInManager.PasswordSignInAsync(
        user,
        password,
        rememberMe,
        lockoutOnFailure: true);

    if (result.Succeeded)
    {
        return Results.Redirect("/");
    }

    if (result.IsLockedOut)
    {
        return Results.Redirect("/login?error=locked");
    }

    if (result.IsNotAllowed)
    {
        return Results.Redirect("/login?error=notallowed");
    }

    return Results.Redirect("/login?error=invalid");
});

app.MapPost("/account/logout", async (
    SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();

    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
