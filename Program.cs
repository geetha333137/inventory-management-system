using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.Services.Implementations;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NotificationButtonOnly_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC
builder.Services.AddControllersWithViews();

// Register notification logic
builder.Services.AddSingleton<INotificationService, NotificationService>();

var sqliteConnection = new SqliteConnection("Data Source=:memory:");
sqliteConnection.Open();

builder.Services.AddSingleton(sqliteConnection);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnection));

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

// =========================================================
// AUTHENTICATION
// =========================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "InventoryCookie";
        options.DefaultSignInScheme = "InventoryCookie";
        options.DefaultChallengeScheme = "InventoryCookie";
    })
    .AddCookie("InventoryCookie", options =>
    {
        options.Cookie.Name =
            "InventoryManagementSystem.InventoryCookie";

        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";
    });

// =========================================================
// AUTHORIZATION
// =========================================================

builder.Services.AddAuthorization();

// =========================================================
// APPLICATION SERVICES
// =========================================================

builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// =========================================================
// DATABASE INITIALIZATION
// =========================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    SeedData.Initialize(db);
    DbInitializer.Seed(db);
}

// =========================================================
// HTTP REQUEST PIPELINE
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Authentication must run before Authorization
app.UseAuthentication();

app.UseAuthorization();

// =========================================================
// ROUTING
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();