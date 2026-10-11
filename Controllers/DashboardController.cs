using System.Security.Claims;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly AppDbContext _context;

        public DashboardController(IDashboardService dashboardService, AppDbContext context)
        {
            _dashboardService = dashboardService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Standard users are redirected to their dedicated user workspace dashboard
            if (User.IsInRole("User") && !User.IsInRole("Asset Manager") && !User.IsInRole("Inventory Executive"))
            {
                return RedirectToAction(nameof(UserDashboard));
            }

            ViewData["Title"] = "Inventory Dashboard";
            ViewData["PageSubtitle"] = "A live overview of asset status, categories and recent activity.";

            var model = await _dashboardService.GetDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> UserDashboard()
        {
            ViewData["Title"] = "User Dashboard";
            ViewData["PageSubtitle"] = "Your assigned equipment, available assets, and account details.";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int userId);
            var userName = User.Identity?.Name ?? string.Empty;
            var fullName = User.FindFirst("FullName")?.Value ?? userName;

            // Load user entity for complete department & contact info
            var userEntity = userId > 0 
                ? await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId) 
                : null;

            var effectiveFullName = userEntity != null && !string.IsNullOrWhiteSpace(userEntity.FullName) 
                ? userEntity.FullName 
                : fullName;

            // Find assets assigned to this user either by ID or name
            var assignedAssets = await _context.Assets
                .Include(a => a.Category)
                .Where(a => (userId > 0 && a.AssignedToUserId == userId)
                         || (!string.IsNullOrEmpty(a.AssignedTo) && 
                            (a.AssignedTo == effectiveFullName || a.AssignedTo == userName)))
                .OrderByDescending(a => a.UpdatedDate ?? a.CreatedDate)
                .ToListAsync();

            var availableAssetsCount = await _context.Assets
                .CountAsync(a => a.Status == AssetStatuses.Available);

            var totalCategoriesCount = await _context.Categories
                .CountAsync(c => c.IsActive);

            var recentAvailableAssets = await _context.Assets
                .Include(a => a.Category)
                .Where(a => a.Status == AssetStatuses.Available)
                .OrderByDescending(a => a.CreatedDate)
                .Take(6)
                .ToListAsync();

            var model = new UserDashboardViewModel
            {
                UserId = userId,
                Username = userName,
                FullName = effectiveFullName,
                Email = userEntity?.Email ?? User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                Department = userEntity?.Department ?? "Operations",
                EmployeeId = userEntity?.EmployeeId ?? "EMP",
                RoleName = userEntity?.Role?.Name ?? "User",
                PhoneNumber = userEntity?.PhoneNumber ?? string.Empty,
                AssignedAssets = assignedAssets,
                AvailableAssetsCount = availableAssetsCount,
                TotalCategoriesCount = totalCategoriesCount,
                RecentAvailableAssets = recentAvailableAssets
            };

            return View(model);
        }
    }
}
