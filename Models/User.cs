namespace InventoryManagementSystem.Models;

public class User
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Username { get; set; } = "";

    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string? MobileNumber { get; set; }

    public string? Department { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public Role Role { get; set; } = null!;

    public ICollection<UserPermission> UserPermissions { get; set; }
        = new List<UserPermission>();

    public string FullName
    {
        get => string.Join(' ', new[] { FirstName, LastName }
            .Where(name => !string.IsNullOrWhiteSpace(name)));
        set
        {
            var parts = value.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            FirstName = parts.ElementAtOrDefault(0) ?? string.Empty;
            LastName = parts.ElementAtOrDefault(1) ?? string.Empty;
        }
    }

    public string? PhoneNumber
    {
        get => MobileNumber;
        set => MobileNumber = value;
    }

    public DateTime CreatedAt
    {
        get => CreatedDate;
        set => CreatedDate = value;
    }
}