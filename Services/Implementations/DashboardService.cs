using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var assets = await _context.Assets
                .Include(a => a.Category)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalAssets = assets.Count,
                ActiveAssets = assets.Count(a => AssetStatuses.Active.Contains(a.Status)),
                InactiveAssets = assets.Count(a => !AssetStatuses.Active.Contains(a.Status)),
                AvailableAssets = CountByStatus(assets, AssetStatuses.Available),
                AssignedAssets = CountByStatus(assets, AssetStatuses.Assigned),
                MaintenanceAssets = CountByStatus(assets, AssetStatuses.UnderMaintenance),
                RetiredAssets = CountByStatus(assets, AssetStatuses.Retired),
                CategoryCount = await _context.Categories.CountAsync(c => c.IsActive),
                AssetsByCategory = assets
                    .GroupBy(a => a.Category?.Name ?? "Uncategorised")
                    .Select(g => new ChartSlice { Label = g.Key, Value = g.Count() })
                    .OrderByDescending(x => x.Value)
                    .ToList(),
                AssetsByStatus = AssetStatuses.All
                    .Select(status => new ChartSlice
                    {
                        Label = status,
                        Value = CountByStatus(assets, status)
                    })
                    .Where(x => x.Value > 0)
                    .ToList(),
                AssetsByLocation = GroupBy(assets, a => a.Location),
                AssetsByDepartment = GroupBy(assets, a => a.Department),
                RecentAssets = assets
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(6)
                    .ToList(),
                RecentActivities = await _context.AuditLogs
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(8)
                    .ToListAsync()
            };

            return viewModel;
        }

        private static int CountByStatus(IEnumerable<Asset> assets, string status) =>
            assets.Count(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));

        private static List<ChartSlice> GroupBy(IEnumerable<Asset> assets, Func<Asset, string?> selector) =>
            assets
                .Select(selector)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .GroupBy(value => value!)
                .Select(g => new ChartSlice { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();
    }
}
