using NotificationButtonOnly_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC.
builder.Services.AddControllersWithViews();

// Register only notification logic.
// Standalone storage uses a JSON file, so no EF Core/SQLite packages are needed.
builder.Services.AddSingleton<INotificationService, NotificationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Notifications/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Root URL -> NotificationsController.Index().
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Notifications}/{action=Index}/{id?}");

app.Run();
