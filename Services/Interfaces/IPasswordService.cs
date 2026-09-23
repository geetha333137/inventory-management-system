using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(
        User user,
        string passwordHash,
        string password);
}