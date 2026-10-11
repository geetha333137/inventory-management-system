using InventoryManagementSystem.Data;
using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [PermissionAuthorize("VIEW_CATEGORY")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Title"] = "Categories";
            ViewData["PageSubtitle"] = "Organise inventory by asset type.";

            var query = _context.Categories
                .Include(c => c.Assets)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(category => category.Name.Contains(term) ||
                                                (category.Description != null && category.Description.Contains(term)));
            }

            ViewBag.Search = search;
            var categories = await query.OrderBy(category => category.Name).ToListAsync();

            return View(categories);
        }

        [PermissionAuthorize("CREATE_ASSET")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Category";
            ViewData["PageSubtitle"] = "Create a category used when registering assets.";
            return View(new Category { IsActive = true });
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            ViewData["Title"] = category.Name;
            ViewData["PageSubtitle"] = "Assets assigned to this category.";
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CREATE_ASSET")]
        public async Task<IActionResult> Create(Category category)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
            {
                ModelState.AddModelError(nameof(category.Name), "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add Category";
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("EDIT_ASSET")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Category";
            ViewData["PageSubtitle"] = "Update category details.";
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("EDIT_ASSET")]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (await _context.Categories.AnyAsync(c => c.Name == category.Name && c.Id != category.Id))
            {
                ModelState.AddModelError(nameof(category.Name), "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Category";
                return View(category);
            }

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("DELETE_ASSET")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Delete Category";
            ViewBag.AssetCount = category.Assets.Count;
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("DELETE_ASSET")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Assets.Any())
            {
                TempData["ErrorMessage"] = "This category cannot be deleted because assets are still assigned to it. Reassign or remove those assets first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
