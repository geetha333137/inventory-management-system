using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IAssetService
    {
        Task<List<Asset>> GetAllAsync();

        Task<Asset?> GetByIdAsync(int id);

        Task<bool> AssetCodeExistsAsync(string assetCode, int? excludeId = null);

        Task CreateAsync(Asset asset, string performedBy);

        Task UpdateAsync(Asset asset, string performedBy);

        Task DeleteAsync(int id, string performedBy);
    }
}
