using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data;

namespace InventoryManagementSystem.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(
        int userId,
        string permissionName)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Role.RolePermissions)
            .AnyAsync(rp =>
                rp.Permission.Name == permissionName);
    }
}