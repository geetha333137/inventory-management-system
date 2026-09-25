using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NotificationButtonOnly_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC.
builder.Services.AddControllersWithViews();

// Register only notification logic.
// Standalone storage uses a JSON file, so no EF Core/SQLite packages are needed.
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

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "InventoryCookie";
        options.DefaultSignInScheme = "InventoryCookie";
        options.DefaultChallengeScheme = "InventoryCookie";
    })
    .AddCookie("InventoryCookie", options =>
    {
        options.Cookie.Name = "InventoryManagementSystem.InventoryCookie";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Home/Index");
    return Task.CompletedTask;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
