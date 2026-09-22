using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels;

public class RoleViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(100)]
    [Display(Name = "Role Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public List<PermissionViewModel> Permissions { get; set; }
        = new List<PermissionViewModel>();
}