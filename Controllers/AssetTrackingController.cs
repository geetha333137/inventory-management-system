using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

[Authorize(Roles = "Asset Manager, Inventory Executive")]
public class AssetTrackingController : Controller
{
    private readonly AppDbContext _context;

    public AssetTrackingController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var assets = await _context.Assets
            .Include(asset => asset.Category)
            .OrderBy(asset => asset.AssetCode)
            .ToListAsync();

        var assetIds = assets.Select(asset => asset.Id).ToList();
        var history = await _context.AuditLogs
            .Where(log => log.EntityName == "Asset" &&
                          log.EntityId.HasValue &&
                          assetIds.Contains(log.EntityId.Value))
            .OrderByDescending(log => log.CreatedDate)
            .ToListAsync();

        var userIds = assets
            .Where(asset => asset.AssignedToUserId.HasValue)
            .Select(asset => asset.AssignedToUserId!.Value)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.FullName);

        var model = new AssetTrackingViewModel
        {
            Assets = assets.Select(asset => new AssetTrackingRow
            {
                AssetId = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                Status = asset.Status,
                Location = asset.Location ?? "Not recorded",
                AssignedUser = ResolveAssignedUser(asset, users),
                Department = asset.Department ?? "Not recorded",
                LastUpdated = asset.UpdatedDate ?? asset.CreatedDate,
                History = history
                    .Where(log => log.EntityId == asset.Id)
                    .Take(8)
                    .ToList()
            }).ToList()
        };

        ViewData["Title"] = "Asset Tracking";
        ViewData["PageSubtitle"] = "Monitor asset status, location, ownership and history.";
        return View(model);
    }

    private static string ResolveAssignedUser(
        Models.Asset asset,
        IReadOnlyDictionary<int, string> users)
    {
        if (asset.AssignedToUserId.HasValue &&
            users.TryGetValue(asset.AssignedToUserId.Value, out var fullName))
        {
            return fullName;
        }

        return string.IsNullOrWhiteSpace(asset.AssignedTo)
            ? "Unassigned"
            : asset.AssignedTo;
    }
}