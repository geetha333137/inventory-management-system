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
            .FirstOrDefault(r => r.Name == "Inventory Executive" || r.Name == "Inventory Viewer");

        if (inventoryExecutiveRole == null)
        {
            inventoryExecutiveRole = new Role
            {
                Name = "Inventory Executive",
                Description = "Operational inventory, asset, and reporting access"
            };

            db.Roles.Add(inventoryExecutiveRole);
            db.SaveChanges();
        }
        else if (inventoryExecutiveRole.Name != "Inventory Executive")
        {
            inventoryExecutiveRole.Name = "Inventory Executive";
            inventoryExecutiveRole.Description = "Operational inventory, asset, and reporting access";
            db.SaveChanges();
        }

        var userRole = db.Roles
            .FirstOrDefault(r => r.Name == "User" || r.Name == "Standard User");

        if (userRole == null)
        {
            userRole = new Role
            {
                Name = "User",
                Description = "Standard employee user with asset visibility and profile access"
            };

            db.Roles.Add(userRole);
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
            "VIEW_CATEGORY",
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
        // CREATE ADMIN USER (Asset Manager)
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
        // CREATE EMPLOYEE USER (Inventory Executive)
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
        // CREATE STANDARD USER (User Login)
        // ============================

        var standardUser = db.Users
            .FirstOrDefault(u => u.Username == "user");

        if (standardUser == null)
        {
            standardUser = new User
            {
                EmployeeId = "EMP003",
                FirstName = "Alex",
                LastName = "Taylor",
                Username = "user",
                Email = "user@inventory.com",
                MobileNumber = "7777777777",
                Department = "Operations",
                RoleId = userRole.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<User>();

            standardUser.PasswordHash =
                passwordHasher.HashPassword(
                    standardUser,
                    "User@123");

            db.Users.Add(standardUser);
            db.SaveChanges();
        }

        // Ensure standard user has assigned demo equipment for dashboard preview
        var userAsset = db.Assets.FirstOrDefault(a => a.AssetCode == "AST-000105");
        if (userAsset != null && (userAsset.AssignedToUserId == null || userAsset.AssignedToUserId == 0))
        {
            userAsset.Status = AssetStatuses.Assigned;
            userAsset.AssignedTo = $"{standardUser.FirstName} {standardUser.LastName}".Trim();
            userAsset.AssignedToUserId = standardUser.Id;
            userAsset.Department = standardUser.Department;
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

        string[] executivePermissions =
        {
            "VIEW_ASSET",
            "CREATE_ASSET",
            "EDIT_ASSET",
            "DELETE_ASSET",
            "VIEW_CATEGORY",
            "VIEW_REPORT",
            "VIEW_NOTIFICATIONS"
        };

        foreach (var permissionName in executivePermissions)
        {
            var permission = db.Permissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission != null)
            {
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
        }

        // ============================
        // STANDARD USER PERMISSIONS
        // ============================

        string[] userPermissions =
        {
            "VIEW_ASSET",
            "VIEW_CATEGORY"
        };

        foreach (var permissionName in userPermissions)
        {
            var permission = db.Permissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission != null)
            {
                bool exists = db.RolePermissions.Any(
                    rp => rp.RoleId == userRole.Id &&
                          rp.PermissionId == permission.Id);

                if (!exists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = userRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        db.SaveChanges();
    }
}