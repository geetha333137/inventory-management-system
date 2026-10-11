using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

[Authorize(Roles = "Asset Manager, Inventory Executive")]
public class AssetAssignmentsController : Controller
{
    private readonly AppDbContext _context;

    public AssetAssignmentsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var assets = await _context.Assets
            .OrderBy(asset => asset.AssetCode)
            .ToListAsync();

        var userIds = assets
            .Where(asset => asset.AssignedToUserId.HasValue)
            .Select(asset => asset.AssignedToUserId!.Value)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.FullName);

        var model = new AssetAssignmentsViewModel
        {
            Assignments = assets.Select(asset => new AssetAssignmentRow
            {
                AssetId = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                AssignedUser = ResolveAssignedUser(asset, users),
                Department = asset.Department ?? "Not recorded",
                AssignmentStatus = IsAssigned(asset) ? "Assigned" : "Unassigned",
                AssignmentDate = asset.UpdatedDate ?? asset.CreatedDate
            }).ToList()
        };

        ViewData["Title"] = "Asset Assignments";
        ViewData["PageSubtitle"] = "Review ownership and manage asset assignments.";
        return View(model);
    }

    private static bool IsAssigned(Models.Asset asset) =>
        asset.AssignedToUserId.HasValue ||
        !string.IsNullOrWhiteSpace(asset.AssignedTo);

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