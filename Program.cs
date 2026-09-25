using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using OrganisationProfile.OrganisationProfile.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("~/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("~/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("~/Views/OrganisationProfile/{0}.cshtml");
    options.ViewLocationFormats.Add("~/Views/OrganisationProfile/{1}/{0}.cshtml");
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Organisation/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Organisation}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
