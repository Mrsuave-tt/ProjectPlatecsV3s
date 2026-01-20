using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    // Password settings - relaxed to allow Student IDs as initial passwords
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4; // Minimum length to allow short Student IDs
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@"; // Allow Student ID format
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// Register Email Service
builder.Services.AddScoped<ProjectPlatec.Services.EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roles = new[] { "Admin", "Teacher", "Student" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed admin user
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User"
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // You can add more admin accounts here by copying the above code and changing the email/password
    // Example:
    // var anotherAdminEmail = "superadmin@example.com";
    // var anotherAdminUser = await userManager.FindByEmailAsync(anotherAdminEmail);
    // if (anotherAdminUser == null)
    // {
    //     anotherAdminUser = new ApplicationUser
    //     {
    //         UserName = anotherAdminEmail,
    //         Email = anotherAdminEmail,
    //         FirstName = "Super",
    //         LastName = "Admin"
    //     };
    //     var result2 = await userManager.CreateAsync(anotherAdminUser, "SuperAdmin456!");
    //     if (result2.Succeeded)
    //     {
    //         await userManager.AddToRoleAsync(anotherAdminUser, "Admin");
    //     }
    // }

    // Assign roles to existing users
    var allUsers = userManager.Users.ToList();
    foreach (var user in allUsers)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Any())
        {
            // Check if user is a student
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var isStudent = await context.Students.AnyAsync(s => s.UserId == user.Id);
            if (isStudent)
            {
                await userManager.AddToRoleAsync(user, "Student");
            }
            else
            {
                await userManager.AddToRoleAsync(user, "Teacher");
            }
        }
    }

    // To promote an existing user to admin, you can uncomment and modify this code:
    // var userToPromote = await userManager.FindByEmailAsync("existinguser@example.com");
    // if (userToPromote != null)
    // {
    //     await userManager.AddToRoleAsync(userToPromote, "Admin");
    // }
}

app.Run();
