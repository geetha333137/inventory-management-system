namespace InventoryManagementSystem.Models;

public class Permission
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();

    public ICollection<UserPermission> UserPermissions { get; set; }
        = new List<UserPermission>();
}