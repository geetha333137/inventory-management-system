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

        var hasRolePermission = await _db.RolePermissions
            .AnyAsync(rp =>
                rp.RoleId == user.RoleId &&
                rp.Permission.Name == permissionName);

        if (hasRolePermission)
        {
            return true;
        }

        return await _db.UserPermissions
            .AnyAsync(up =>
                up.UserId == userId &&
                up.Permission.Name == permissionName);
    }
}