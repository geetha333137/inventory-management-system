using InventoryManagementSystem.Data;
using InventoryManagementSystem.Modules;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

[Authorize(Roles = "Asset Manager")]
public class RolesController : Controller
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Roles
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return View(roles);
    }

    // GET: /Roles/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoleViewModel());
    }

    // POST: /Roles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var roleExists = await _context.Roles
            .AnyAsync(r => r.Name == model.Name);

        if (roleExists)
        {
            ModelState.AddModelError(
                "Name",
                "A role with this name already exists.");

            return View(model);
        }

        var role = new Role
        {
            Name = model.Name,
            Description = model.Description
        };

        _context.Roles.Add(role);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /Roles/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        var model = new RoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };

        return View(model);
    }

    // POST: /Roles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        RoleViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        var duplicateName = await _context.Roles
            .AnyAsync(r =>
                r.Name == model.Name &&
                r.Id != id);

        if (duplicateName)
        {
            ModelState.AddModelError(
                "Name",
                "A role with this name already exists.");

            return View(model);
        }

        role.Name = model.Name;
        role.Description = model.Description;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /Roles/Permissions/5
    [HttpGet]
    public async Task<IActionResult> Permissions(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        var permissions = await _context.Permissions
            .OrderBy(p => p.Name)
            .ToListAsync();

        var selectedPermissionIds = role.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var model = new RoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,

            Permissions = permissions
                .Select(p => new PermissionViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    IsSelected = selectedPermissionIds.Contains(p.Id)
                })
                .ToList()
        };

        return View(model);
    }

    // POST: /Roles/Permissions
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(
        int id,
        int[] selectedPermissionIds)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        // Remove existing permissions
        _context.RolePermissions.RemoveRange(
            role.RolePermissions);

        // Add selected permissions
        foreach (var permissionId in selectedPermissionIds.Distinct())
        {
            _context.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}