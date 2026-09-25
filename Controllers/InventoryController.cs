using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public IActionResult Create()
        {
            return View(new InventoryItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
