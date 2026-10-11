using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            if (User.IsInRole("Asset Manager") || User.IsInRole("Inventory Executive"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return RedirectToAction("UserDashboard", "Dashboard");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}