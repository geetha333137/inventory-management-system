using InvertoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvertoryManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Inventory Dashboard";
            ViewData["PageSubtitle"] = "A live overview of asset status, categories and recent activity.";

            var model = await _dashboardService.GetDashboardAsync();
            return View(model);
        }
    }
}
