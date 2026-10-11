using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ViewModels;

public class AssetTrackingViewModel
{
    public List<AssetTrackingRow> Assets { get; set; } = new();
}

public class AssetTrackingRow
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AssignedUser { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public List<AuditLog> History { get; set; } = new();
}