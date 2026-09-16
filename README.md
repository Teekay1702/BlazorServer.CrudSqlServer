# HR Management System - Blazor Server + Identity + Full HR Suite

Secure HR platform with ASP.NET Core Identity, role-based access, and full employee lifecycle.

## Identity & Auth

**Packages added:**
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.11
- Microsoft.AspNetCore.Identity.UI 8.0.11
- Microsoft.AspNetCore.Components.Authorization 8.0.11

**ApplicationUser extends IdentityUser:**
- FullName, EmployeeId FK, DepartmentId FK, CreatedAt, IsActive, ProfilePictureUrl
- Linked to Employee and Department tables

**Roles (seeded automatically in SeedData.cs):**
- Admin - full access + Users page
- HRManager - HR operations (salaries, leave approval, payroll)
- Manager - team view
- Employee - self-service

**Default accounts (auto-created on first run):**
```
Admin: admin@hrsystem.local / Admin@123
HR:    hr@hrsystem.local / Hr@12345
```

**Auth flow:**
- Program.cs: AddCascadingAuthenticationState, AddIdentity<ApplicationUser, IdentityRole>, AddAuthorization with policies
- UseAuthentication() + UseAuthorization() middleware
- Routes.razor uses AuthorizeRouteView with NotAuthorized handling
- NavMenu shows signed-in user + roles + Sign Out, or Sign In/Register if anonymous
- All HR pages have [Authorize], Users page has [Authorize(Roles="Admin")]
- Home page is public but shows dashboard only if Authorized, otherwise landing with demo creds

**Pages:**
- /login - Sign in with email/password, remember me, demo creds hint
- /register - Create account with FullName, link to Employee/Department, role selection (Admin only if current user is Admin)
- /logout - Signs out and redirects
- /users - Admin only: list all users, edit FullName, link Employee/Dept, edit roles (comma separated), active toggle, delete

## HR Modules (previous)

**Migrations:**
- InitialCreate, AddDepartmentsAndSalaries, AddLeaveAttendancePayroll, AddIdentity

**Core:** Departments (seeded 5), Employees (code auto), Salaries (history)
**Leave:** LeaveRequest with overlapping validation, approval workflow
**Attendance:** Daily check-in/out, late/OT detection, bulk present
**Payroll:** Monthly generation from salaries, process/paid workflow

## Setup

1. Connection string in appsettings.json
2. `dotnet run`
   - Migrate() creates DB + all tables including AspNet* Identity tables
   - SeedData creates roles + 2 default users
3. Sign in with admin@hrsystem.local / Admin@123
4. Go to /users to manage users, /register to create more

## Security Notes

- Password policy: 6+ chars, upper, lower, digit (relaxed for dev)
- Lockout: 5 attempts, 5 min
- Email confirmation disabled for dev (enable for prod)
- Computed properties fixed with [NotMapped] to avoid EF mapping errors (Period, NetSalary, HoursWorked, etc.)

## Tech

- .NET 8 Blazor Server InteractiveServer
- EF Core 8.0.11 SqlServer + Identity
- Bootstrap 5
- Role-based authorization
