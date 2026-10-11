using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync(string? search);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetProfileAsync(int id);
        Task<List<Role>> GetRolesAsync();
        Task<bool> CreateUserAsync(UserCreateViewModel model);
        Task<bool> UpdateUserAsync(UserEditViewModel model);
        Task<bool> UpdateOwnProfileAsync(int id, EditProfileViewModel model);
        Task<bool> ToggleUserStatusAsync(int id);
    }
}