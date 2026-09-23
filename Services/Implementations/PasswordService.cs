using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        User user,
        string passwordHash,
        string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}