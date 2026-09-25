namespace InventoryManagementSystem.Models
{
    public static class AssetStatuses
    {
        public const string Available = "Available";
        public const string Assigned = "Assigned";
        public const string UnderMaintenance = "Under Maintenance";
        public const string Damaged = "Damaged";
        public const string Retired = "Retired";
        public const string Lost = "Lost";

        public static readonly string[] All =
        {
            Available,
            Assigned,
            UnderMaintenance,
            Damaged,
            Retired,
            Lost
        };

        public static readonly string[] Active =
        {
            Available,
            Assigned,
            UnderMaintenance
        };

        public static string BadgeClass(string? status) => status switch
        {
            Available => "badge-available",
            Assigned => "badge-assigned",
            UnderMaintenance => "badge-maintenance",
            Damaged => "badge-damaged",
            Retired => "badge-retired",
            Lost => "badge-lost",
            _ => "badge text-bg-secondary"
        };
    }
}
