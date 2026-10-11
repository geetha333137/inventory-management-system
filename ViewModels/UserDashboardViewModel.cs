using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ViewModels;

public class UserDashboardViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<Asset> AssignedAssets { get; set; } = new();
    public int TotalAssignedAssets => AssignedAssets.Count;
    public int AvailableAssetsCount { get; set; }
    public int TotalCategoriesCount { get; set; }
    public List<Asset> RecentAvailableAssets { get; set; } = new();
}
