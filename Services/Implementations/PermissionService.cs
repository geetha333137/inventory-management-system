using InventoryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _db;

    public PermissionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(
        int userId,
        string permissionName)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false;
        }

        return await _db.RolePermissions
            .Include(rp => rp.Permission)
            .AnyAsync(rp =>
                rp.RoleId == user.RoleId &&
                rp.Permission.Name == permissionName);
    }
}