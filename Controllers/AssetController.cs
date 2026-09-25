using InvertoryManagementSystem.Data;
using InvertoryManagementSystem.Models;
using InvertoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InvertoryManagementSystem.Controllers
{
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly AppDbContext _context;

        public AssetController(IAssetService assetService, AppDbContext context)
        {
            _assetService = assetService;
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? category,
            string? location,
            string? department,
            string? assignedTo)
        {
            ViewData["Title"] = "Inventory";
            ViewData["PageSubtitle"] = "Search, filter and maintain the organisation asset register.";

            var allAssets = await _assetService.GetAllAsync();

            ViewBag.TotalAssets = allAssets.Count;
            ViewBag.AvailableAssets = allAssets.Count(a => a.Status == AssetStatuses.Available);
            ViewBag.AssignedAssets = allAssets.Count(a => a.Status == AssetStatuses.Assigned);
            ViewBag.MaintenanceAssets = allAssets.Count(a => a.Status == AssetStatuses.UnderMaintenance);

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Locations = DistinctValues(allAssets.Select(a => a.Location));
            ViewBag.Departments = DistinctValues(allAssets.Select(a => a.Department));
            ViewBag.AssignedUsers = DistinctValues(allAssets.Select(a => a.AssignedTo));

            var assets = allAssets.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                assets = assets.Where(a =>
                    a.AssetCode.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || a.AssetName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(a.SerialNumber) && a.SerialNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(a.Location) && a.Location.Contains(search, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(a.AssignedTo) && a.AssignedTo.Contains(search, StringComparison.OrdinalIgnoreCase))
                    || (a.Category != null && a.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                assets = assets.Where(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                assets = assets.Where(a => a.Category != null
                    && string.Equals(a.Category.Name, category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                assets = assets.Where(a => string.Equals(a.Location, location, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                assets = assets.Where(a => string.Equals(a.Department, department, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                assets = assets.Where(a => string.Equals(a.AssignedTo, assignedTo, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Category = category;
            ViewBag.Location = location;
            ViewBag.Department = department;
            ViewBag.AssignedTo = assignedTo;

            return View(assets.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var asset = await _assetService.GetByIdAsync(id);

            if (asset == null)
            {
                TempData["ErrorMessage"] = "The requested asset could not be found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Asset Details";
            ViewData["PageSubtitle"] = "Complete record for this inventory item.";
            return View(asset);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Add Asset";
            ViewData["PageSubtitle"] = "Register a new asset in the inventory.";
            await LoadCategoriesAsync();
            return View(new Asset { Status = AssetStatuses.Available });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            ValidateAsset(asset);

            if (await _assetService.AssetCodeExistsAsync(asset.AssetCode))
            {
                ModelState.AddModelError(nameof(asset.AssetCode), "An asset with this identification number already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add Asset";
                await LoadCategoriesAsync(asset.CategoryId);
                return View(asset);
            }

            try
            {
                await _assetService.CreateAsync(asset, CurrentUser());
                TempData["SuccessMessage"] = "Asset added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Unable to save the asset. Please verify the entered information and try again.");
                ViewData["Title"] = "Add Asset";
                await LoadCategoriesAsync(asset.CategoryId);
                return View(asset);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var asset = await _assetService.GetByIdAsync(id);

            if (asset == null)
            {
                TempData["ErrorMessage"] = "The requested asset could not be found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Edit Asset";
            ViewData["PageSubtitle"] = "Update asset information and assignment details.";
            await LoadCategoriesAsync(asset.CategoryId);
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Asset asset)
        {
            if (id != asset.Id)
            {
                return BadRequest();
            }

            ValidateAsset(asset);

            if (await _assetService.AssetCodeExistsAsync(asset.AssetCode, asset.Id))
            {
                ModelState.AddModelError(nameof(asset.AssetCode), "Another asset already uses this identification number.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Asset";
                await LoadCategoriesAsync(asset.CategoryId);
                return View(asset);
            }

            try
            {
                await _assetService.UpdateAsync(asset, CurrentUser());
                TempData["SuccessMessage"] = "Asset updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Unable to save the asset. Please verify the entered information and try again.");
                ViewData["Title"] = "Edit Asset";
                await LoadCategoriesAsync(asset.CategoryId);
                return View(asset);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await _assetService.GetByIdAsync(id);

            if (asset == null)
            {
                TempData["ErrorMessage"] = "The requested asset could not be found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Delete Asset";
            ViewData["PageSubtitle"] = "Confirm that this asset should be removed from the register.";
            return View(asset);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _assetService.GetByIdAsync(id);

            if (asset == null)
            {
                TempData["ErrorMessage"] = "The requested asset could not be found.";
                return RedirectToAction(nameof(Index));
            }

            await _assetService.DeleteAsync(id, CurrentUser());
            TempData["SuccessMessage"] = "Asset deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidateAsset(Asset asset)
        {
            if (string.Equals(asset.Status, AssetStatuses.Assigned, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(asset.AssignedTo))
            {
                ModelState.AddModelError(nameof(asset.AssignedTo), "Assigned To is required when the asset status is Assigned.");
            }

            if (asset.PurchaseDate.HasValue && asset.PurchaseDate.Value.Date > DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(asset.PurchaseDate), "Purchase date cannot be in the future.");
            }
        }

        private async Task LoadCategoriesAsync(int? selectedCategoryId = null)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }

        private static List<string> DistinctValues(IEnumerable<string?> values) =>
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();

        private string CurrentUser() =>
            User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "Inventory User";
    }
}
