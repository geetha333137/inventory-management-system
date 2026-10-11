using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels;

public class EditProfileViewModel
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    [Display(Name = "Phone")]
    public string? PhoneNumber { get; set; }
}