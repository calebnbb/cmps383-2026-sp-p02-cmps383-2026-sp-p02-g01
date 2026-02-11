using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Domain.Identity;
using Selu383.SP26.Api.Features.Locations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

builder.Services
    .AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

// Important for APIs: don't redirect to login/access denied; return proper status codes.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations + seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<DataContext>();
    db.Database.Migrate();

    // Seed Locations (your existing seed)
    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10 },
            new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20 },
            new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15 }
        );
        db.SaveChanges();
    }

    // Seed Identity (roles + users)
    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await SeedIdentityAsync(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedIdentityAsync(RoleManager<Role> roleManager, UserManager<User> userManager)
{
    // Roles
    await EnsureRoleAsync(roleManager, "Admin");
    await EnsureRoleAsync(roleManager, "User");

    // Users (Password: Password123!)
    await EnsureUserWithRoleAsync(userManager, "galkadi", "Password123!", "Admin");
    await EnsureUserWithRoleAsync(userManager, "bob", "Password123!", "User");
    await EnsureUserWithRoleAsync(userManager, "sue", "Password123!", "User");
}

static async Task EnsureRoleAsync(RoleManager<Role> roleManager, string roleName)
{
    if (await roleManager.RoleExistsAsync(roleName))
        return;

    var result = await roleManager.CreateAsync(new Role { Name = roleName });
    if (!result.Succeeded)
        throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
}

static async Task EnsureUserWithRoleAsync(UserManager<User> userManager, string username, string password, string roleName)
{
    var user = await userManager.FindByNameAsync(username);

    if (user is null)
    {
        user = new User
        {
            UserName = username,
            Email = $"{username}@example.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new Exception($"Failed to create user '{username}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
    }

    if (!await userManager.IsInRoleAsync(user, roleName))
    {
        var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!addRoleResult.Succeeded)
            throw new Exception($"Failed to add user '{username}' to role '{roleName}': {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
    }
}

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }
