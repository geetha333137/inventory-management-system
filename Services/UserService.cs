using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<List<User>> GetUsersAsync(string? search)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search));
            }

            return await query
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetProfileAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<bool> CreateUserAsync(UserCreateViewModel model)
        {
            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
                return false;

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
                return false;

            var user = new User
            {
                Username = model.Username.Trim(),
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                RoleId = model.RoleId,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                model.Password
            );

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUserAsync(UserEditViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null)
                return false;

            bool usernameExists = await _context.Users
                .AnyAsync(u =>
                    u.Username == model.Username &&
                    u.Id != model.Id);

            if (usernameExists)
                return false;

            bool emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == model.Email &&
                    u.Id != model.Id);

            if (emailExists)
                return false;

            user.Username = model.Username.Trim();
            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateOwnProfileAsync(
            int id,
            EditProfileViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            var username = model.Username.Trim();
            var email = model.Email.Trim();

            var duplicate = await _context.Users.AnyAsync(u =>
                u.Id != id &&
                (u.Username == username || u.Email == email));

            if (duplicate)
            {
                return false;
            }

            user.Username = username;
            user.FullName = model.FullName.Trim();
            user.Email = email;
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            user.IsActive = !user.IsActive;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}