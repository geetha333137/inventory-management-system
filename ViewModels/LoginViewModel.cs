namespace InventoryManagementSystem.ViewModels;

public class LoginViewModel
{
    public string UsernameOrEmail { get; set; } = "";

    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}