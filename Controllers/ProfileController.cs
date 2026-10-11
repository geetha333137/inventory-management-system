using System.Security.Claims;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetProfileAsync(CurrentUserId());
        if (user == null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Profile";
        ViewData["PageSubtitle"] = "Manage your personal StockSphere information.";

        return View(new ProfileViewModel
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = user.Role?.Name ?? "Unassigned"
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userService.GetProfileAsync(CurrentUserId());
        if (user == null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Profile";
        return View(new EditProfileViewModel
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Profile";
            return View(model);
        }

        var updated = await _userService.UpdateOwnProfileAsync(
            CurrentUserId(),
            model);

        if (!updated)
        {
            ModelState.AddModelError(
                string.Empty,
                "Username or email is already in use.");
            ViewData["Title"] = "Edit Profile";
            return View(model);
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private int CurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}