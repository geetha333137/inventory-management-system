using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services;

public interface IAuthenticationService
{
    Task<User?> ValidateUserAsync(
        string usernameOrEmail,
        string password);
}