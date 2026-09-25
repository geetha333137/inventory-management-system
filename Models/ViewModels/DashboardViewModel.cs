namespace InvertoryManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int InactiveAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int AssignedAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }
        public int CategoryCount { get; set; }

        public List<ChartSlice> AssetsByCategory { get; set; } = new();
        public List<ChartSlice> AssetsByStatus { get; set; } = new();
        public List<ChartSlice> AssetsByLocation { get; set; } = new();
        public List<ChartSlice> AssetsByDepartment { get; set; } = new();

        public List<Asset> RecentAssets { get; set; } = new();
        public List<AuditLog> RecentActivities { get; set; } = new();
    }

    public class ChartSlice
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
