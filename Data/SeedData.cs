using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        // ============================
        // CREATE ROLES
        // ============================

        var assetManagerRole = db.Roles
            .FirstOrDefault(r => r.Name == "Asset Manager");

        if (assetManagerRole == null)
        {
            assetManagerRole = new Role
            {
                Name = "Asset Manager",
                Description = "Full system access"
            };

            db.Roles.Add(assetManagerRole);
            db.SaveChanges();
        }

        var inventoryExecutiveRole = db.Roles
            .FirstOrDefault(r => r.Name == "Inventory Executive");

        if (inventoryExecutiveRole == null)
        {
            inventoryExecutiveRole = new Role
            {
                Name = "Inventory Executive",
                Description = "Inventory and reporting access"
            };

            db.Roles.Add(inventoryExecutiveRole);
            db.SaveChanges();
        }

        // ============================
        // CREATE PERMISSIONS
        // ============================

        string[] permissionNames =
        {
            "VIEW_ASSET",
            "CREATE_ASSET",
            "EDIT_ASSET",
            "DELETE_ASSET",
            "VIEW_REPORT",
            "MANAGE_USERS",
            "MANAGE_ROLES",
            "MANAGE_PERMISSIONS",
            "MANAGE_ORGANIZATION",
            "VIEW_NOTIFICATIONS"
        };

        foreach (var permissionName in permissionNames)
        {
            if (!db.Permissions.Any(p => p.Name == permissionName))
            {
                db.Permissions.Add(new Permission
                {
                    Name = permissionName,
                    Description = permissionName
                });
            }
        }

        db.SaveChanges();

        // ============================
        // CREATE ADMIN USER
        // ============================

        var admin = db.Users
            .FirstOrDefault(u => u.Username == "admin");

        if (admin == null)
        {
            admin = new User
            {
                EmployeeId = "EMP001",
                FirstName = "System",
                LastName = "Admin",
                Username = "admin",
                Email = "admin@inventory.com",
                MobileNumber = "9999999999",
                Department = "Administration",
                RoleId = assetManagerRole.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<User>();

            admin.PasswordHash =
                passwordHasher.HashPassword(
                    admin,
                    "Admin@123");

            db.Users.Add(admin);

            db.SaveChanges();
        }

        // ============================
        // CREATE EMPLOYEE USER
        // ============================

        var employee = db.Users
            .FirstOrDefault(u => u.Username == "employee");

        if (employee == null)
        {
            employee = new User
            {
                EmployeeId = "EMP002",
                FirstName = "Inventory",
                LastName = "Executive",
                Username = "employee",
                Email = "employee@inventory.com",
                MobileNumber = "8888888888",
                Department = "Inventory",
                RoleId = inventoryExecutiveRole.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<User>();

            employee.PasswordHash =
                passwordHasher.HashPassword(
                    employee,
                    "Employee@123");

            db.Users.Add(employee);

            db.SaveChanges();
        }

        // ============================
        // ASSET MANAGER PERMISSIONS
        // ============================

        var allPermissions = db.Permissions.ToList();

        foreach (var permission in allPermissions)
        {
            bool exists = db.RolePermissions.Any(
                rp => rp.RoleId == assetManagerRole.Id &&
                      rp.PermissionId == permission.Id);

            if (!exists)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = assetManagerRole.Id,
                    PermissionId = permission.Id
                });
            }
        }

        // ============================
        // INVENTORY EXECUTIVE PERMISSIONS
        // ============================

        string[] employeePermissions =
        {
            "VIEW_ASSET",
            "CREATE_ASSET",
            "EDIT_ASSET",
            "DELETE_ASSET",
            "VIEW_REPORT",
            "VIEW_NOTIFICATIONS"
        };

        foreach (var permissionName in employeePermissions)
        {
            var permission = db.Permissions
                .First(p => p.Name == permissionName);

            bool exists = db.RolePermissions.Any(
                rp => rp.RoleId == inventoryExecutiveRole.Id &&
                      rp.PermissionId == permission.Id);

            if (!exists)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = inventoryExecutiveRole.Id,
                    PermissionId = permission.Id
                });
            }
        }

        db.SaveChanges();
    }
}