using System.Security.Claims;
using System.Security.Cryptography;

using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels;
using InventoryManagementSystem.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AuthenticationServiceInterface =
    InventoryManagementSystem.Services.IAuthenticationService;

namespace InventoryManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly AuthenticationServiceInterface _authenticationService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;
    private readonly AppDbContext _db;
    private readonly IPermissionService _permissionService;

    public AccountController(
        AuthenticationServiceInterface authenticationService,
        IPasswordService passwordService,
        IEmailService emailService,
        ILogger<AccountController> logger,
        AppDbContext db,
        IPermissionService permissionService)
    {
        _authenticationService = authenticationService;
        _passwordService = passwordService;
        _emailService = emailService;
        _logger = logger;
        _db = db;
        _permissionService = permissionService;
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _authenticationService.ValidateUserAsync(
            model.UsernameOrEmail,
            model.Password);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username/email or password.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role.Name),

            new Claim(
                "FullName",
                $"{user.FirstName} {user.LastName}")
        };

        var identity = new ClaimsIdentity(
            claims,
            "InventoryCookie");

        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe
        };

        if (model.RememberMe)
        {
            authProperties.ExpiresUtc =
                DateTimeOffset.UtcNow.AddDays(7);
        }
        else
        {
            authProperties.ExpiresUtc =
                DateTimeOffset.UtcNow.AddMinutes(30);
        }

        await HttpContext.SignInAsync(
            "InventoryCookie",
            principal,
            authProperties);

        if (await _permissionService.HasPermissionAsync(
            user.Id,
            "VIEW_ASSET"))
        {
            return RedirectToAction(
            "Index",
            "Dashboard");
        }

        if (await _permissionService.HasPermissionAsync(
            user.Id,
            "MANAGE_USERS"))
        {
            return RedirectToAction(
            "Index",
            "Users");
        }

        return RedirectToAction(nameof(AccessDenied));
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            "InventoryCookie");

        return RedirectToAction(
            "Login",
            "Account");
    }

    // =========================================================
    // CHANGE PASSWORD - GET
    // =========================================================

    [HttpGet]
    public IActionResult ChangePassword()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login");
        }

        return View();
    }

    // =========================================================
    // CHANGE PASSWORD - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get logged-in user's ID
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return RedirectToAction("Login");
        }

        if (!int.TryParse(
                userIdClaim.Value,
                out int userId))
        {
            return RedirectToAction("Login");
        }

        // Find user
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Verify current password
        bool currentPasswordValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                model.CurrentPassword);

        if (!currentPasswordValid)
        {
            ModelState.AddModelError(
                "CurrentPassword",
                "Current password is incorrect.");

            return View(model);
        }

        // Hash new password
        user.PasswordHash =
            _passwordService.HashPassword(
                user,
                model.NewPassword);

        // Update date
        user.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Password changed successfully.";

        return RedirectToAction(
            "Dashboard",
            "Home");
    }

    // =========================================================
    // FORGOT PASSWORD - GET
    // =========================================================

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // =========================================================
    // FORGOT PASSWORD - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Find user using email
        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.Email == model.Email);

        // If email doesn't exist, stop here.
        if (user == null)
        {
            // Do not reveal whether an email exists in the system.
            TempData["ResetMessage"] =
                "If an account exists with this email, " +
                "a password reset link has been sent.";

            return RedirectToAction(
                "ForgotPassword");
        }

        // Generate secure random token
        byte[] tokenBytes =
            RandomNumberGenerator.GetBytes(32);

        string token =
            Convert.ToBase64String(tokenBytes);

        // Create password reset token
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,

            Token = token,

            // Token valid for 30 minutes
            ExpiresAt =
                DateTime.UtcNow.AddMinutes(30),

            IsUsed = false,

            CreatedAt =
                DateTime.UtcNow
        };

        var resetUrl = Url.Action(
            "ResetPassword",
            "Account",
            new { token },
            Request.Scheme,
            Request.Host.Value);

        if (string.IsNullOrWhiteSpace(resetUrl))
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to create a password reset link.");

            return View(model);
        }

        try
        {
            await _emailService.SendPasswordResetAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}".Trim(),
                resetUrl,
                HttpContext.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to send a password reset email to {Email}.",
                user.Email);

            ModelState.AddModelError(
                string.Empty,
                "We could not send the reset email right now. Please try again later.");

            return View(model);
        }

        // Store the token only after SMTP accepts the message.
        _db.PasswordResetTokens.Add(resetToken);
        await _db.SaveChangesAsync();

        TempData["ResetMessage"] =
            "If an account exists with this email, " +
            "a password reset link has been sent.";

        return RedirectToAction(
            "ForgotPassword");
    }

    // =========================================================
    // ACCESS DENIED
    // =========================================================


[HttpGet]
public IActionResult ResetPassword(string token)
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return RedirectToAction("ForgotPassword");
    }

    var model = new ResetPasswordViewModel
    {
        Token = token
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Find the reset token
    var resetToken = await _db.PasswordResetTokens
        .Include(t => t.User)
        .FirstOrDefaultAsync(t => t.Token == model.Token);

    // Check token validity
    if (resetToken == null ||
        resetToken.IsUsed ||
        resetToken.ExpiresAt < DateTime.UtcNow)
    {
        ModelState.AddModelError(
            string.Empty,
            "This password reset link is invalid or has expired.");

        return View(model);
    }

    // Get the user associated with the token
    var user = resetToken.User;

    // Hash the new password
    user.PasswordHash =
        _passwordService.HashPassword(
            user,
            model.NewPassword);

    user.UpdatedDate = DateTime.UtcNow;

    // Mark token as used
    resetToken.IsUsed = true;

    await _db.SaveChangesAsync();

    TempData["SuccessMessage"] =
        "Your password has been reset successfully. Please login.";

    return RedirectToAction(
        "Login",
        "Account");
}

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}