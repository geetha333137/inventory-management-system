namespace InventoryManagementSystem.ViewModels;

public class AssetAssignmentsViewModel
{
    public List<AssetAssignmentRow> Assignments { get; set; } = new();
}

public class AssetAssignmentRow
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssignedUser { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string AssignmentStatus { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; }
}