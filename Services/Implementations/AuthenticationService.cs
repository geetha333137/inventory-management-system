using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwordService;

    public AuthenticationService(
        AppDbContext db,
        IPasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    public async Task<User?> ValidateUserAsync(
        string usernameOrEmail,
        string password)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u =>
                u.Username == usernameOrEmail ||
                u.Email == usernameOrEmail);

        Console.WriteLine("================================");
        Console.WriteLine("LOGIN DEBUG");
        Console.WriteLine($"Entered: {usernameOrEmail}");
        Console.WriteLine($"User found: {user != null}");

        if (user != null)
        {
            Console.WriteLine($"Username: {user.Username}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Active: {user.IsActive}");
            Console.WriteLine($"Role: {user.Role?.Name}");
            Console.WriteLine(
                $"Password hash exists: {!string.IsNullOrEmpty(user.PasswordHash)}");
        }

        Console.WriteLine("================================");

        if (user == null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null;
        }

        bool passwordValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                password);

        Console.WriteLine($"Password valid: {passwordValid}");

        if (!passwordValid)
        {
            return null;
        }

        return user;
    }
}