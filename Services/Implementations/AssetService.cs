using InvertoryManagementSystem.Data;
using InvertoryManagementSystem.Models;
using InvertoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvertoryManagementSystem.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly AppDbContext _context;

        public AssetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Asset>> GetAllAsync()
        {
            return await _context.Assets
                .Include(a => a.Category)
                .OrderByDescending(a => a.UpdatedDate ?? a.CreatedDate)
                .ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _context.Assets
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> AssetCodeExistsAsync(string assetCode, int? excludeId = null)
        {
            var query = _context.Assets.Where(a => a.AssetCode == assetCode);

            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task CreateAsync(Asset asset, string performedBy)
        {
            asset.CreatedDate = DateTime.UtcNow;
            asset.CreatedByName = performedBy;
            asset.UpdatedDate = asset.CreatedDate;
            asset.UpdatedByName = performedBy;

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            await AddAuditAsync(
                "Created",
                asset.Id,
                performedBy,
                $"New Inventory Added: {asset.AssetName} – {asset.AssetCode} has been added to the inventory.");
        }

        public async Task UpdateAsync(Asset asset, string performedBy)
        {
            var existingAsset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == asset.Id);

            if (existingAsset == null)
            {
                return;
            }

            existingAsset.AssetCode = asset.AssetCode;
            existingAsset.AssetName = asset.AssetName;
            existingAsset.CategoryId = asset.CategoryId;
            existingAsset.Description = asset.Description;
            existingAsset.SerialNumber = asset.SerialNumber;
            existingAsset.PurchaseDate = asset.PurchaseDate;
            existingAsset.PurchaseCost = asset.PurchaseCost;
            existingAsset.Location = asset.Location;
            existingAsset.Department = asset.Department;
            existingAsset.AssignedTo = asset.AssignedTo;
            existingAsset.AssignedToUserId = asset.AssignedToUserId;
            existingAsset.Status = asset.Status;
            existingAsset.UpdatedDate = DateTime.UtcNow;
            existingAsset.UpdatedByName = performedBy;

            await _context.SaveChangesAsync();

            await AddAuditAsync(
                "Updated",
                existingAsset.Id,
                performedBy,
                $"Inventory Updated: {existingAsset.AssetName} – {existingAsset.AssetCode} has been updated.");
        }

        public async Task DeleteAsync(int id, string performedBy)
        {
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                return;
            }

            var name = asset.AssetName;
            var code = asset.AssetCode;

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            await AddAuditAsync(
                "Deleted",
                id,
                performedBy,
                $"Inventory Deleted: {name} – {code} has been removed from the inventory.");
        }

        private async Task AddAuditAsync(string action, int entityId, string performedBy, string description)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action,
                EntityName = "Asset",
                EntityId = entityId,
                PerformedBy = performedBy,
                Description = description,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
