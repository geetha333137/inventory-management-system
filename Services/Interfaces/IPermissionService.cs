using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        int userId,
        string permissionName);
}