using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SmartPortfolioManager.Components;
using SmartPortfolioManager.Data;
using SmartPortfolioManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddRadzenComponents();

var app = builder.Build();

// Create default roles and Admin account
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }

    if (await userManager.FindByEmailAsync("admin@portfolio.com") == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = "admin@portfolio.com",
            Email = "admin@portfolio.com"
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Authentication endpoints
app.MapPost("/api/auth/login", async (
    [FromServices] SignInManager<IdentityUser> signInManager,
    [FromForm] string email,
    [FromForm] string password) =>
{
    var result = await signInManager.PasswordSignInAsync(
        email,
        password,
        isPersistent: false,
        lockoutOnFailure: false);

    if (result.Succeeded)
    {
        return Results.Redirect("/dashboard");
    }

    return Results.Redirect("/login?error=Invalid+credentials");
}).DisableAntiforgery();

app.MapPost("/api/auth/register", async (
    [FromServices] UserManager<IdentityUser> userManager,
    [FromForm] string email,
    [FromForm] string password) =>
{
    var existingUser = await userManager.FindByEmailAsync(email);

    if (existingUser != null)
    {
        return Results.Redirect("/register?error=Email+already+exists");
    }

    var newUser = new IdentityUser
    {
        UserName = email,
        Email = email
    };

    var result = await userManager.CreateAsync(newUser, password);

    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(newUser, "User");
        return Results.Redirect("/login");
    }

    return Results.Redirect("/register?error=Registration+failed");
}).DisableAntiforgery();

app.MapPost("/api/auth/logout", async (
    [FromServices] SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
}).DisableAntiforgery();

app.Run();