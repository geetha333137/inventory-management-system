using InventoryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Modules;

namespace InventoryManagementSystem.Controllers;

[Authorize(Roles = "Asset Manager")]
public class PermissionsController : Controller
{
    private readonly AppDbContext _context;

    public PermissionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(permissions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
        int roleId,
        int permissionId)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp =>
                rp.RoleId == roleId &&
                rp.PermissionId == permissionId);

        if (!exists)
        {
            _context.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Roles");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(
        int roleId,
        int permissionId)
    {
        var rolePermission =
            await _context.RolePermissions
                .FirstOrDefaultAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Roles");
    }
}