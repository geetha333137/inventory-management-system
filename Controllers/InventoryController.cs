using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        [PermissionAuthorize("VIEW_ASSET")]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.InventoryItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(i =>
                    i.Name.Contains(term) ||
                    i.Category.Contains(term));
            }

            var items = await query
                .OrderBy(i => i.Name)
                .ToListAsync();

            ViewBag.Search = search;
            return View(items);
        }

        [PermissionAuthorize("VIEW_ASSET")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Inventory Item Details";
            ViewData["PageSubtitle"] = "Review stock information and item status.";
            return View(item);
        }

        [PermissionAuthorize("CREATE_ASSET")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Inventory Item";
            return View(new InventoryItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CREATE_ASSET")]
        public async Task<IActionResult> Create(InventoryItem model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UpdatedAt = DateTime.UtcNow;
            _context.InventoryItems.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Inventory item created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [PermissionAuthorize("EDIT_ASSET")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Inventory Item";
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("EDIT_ASSET")]
        public async Task<IActionResult> Edit(int id, InventoryItem model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.Name = model.Name;
            item.Category = model.Category;
            item.Quantity = model.Quantity;
            item.Price = model.Price;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Inventory item updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("DELETE_ASSET")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Inventory item removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
