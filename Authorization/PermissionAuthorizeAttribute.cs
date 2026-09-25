using System.Security.Claims;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventoryManagementSystem.Authorization;

public class PermissionAuthorizeAttribute : TypeFilterAttribute
{
    public PermissionAuthorizeAttribute(string permission)
        : base(typeof(PermissionAuthorizeFilter))
    {
        Arguments = new object[] { permission };
    }
}

public class PermissionAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionService _permissionService;
    private readonly string _permission;

    public PermissionAuthorizeFilter(
        IPermissionService permissionService,
        string permission)
    {
        _permissionService = permissionService;
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(
        AuthorizationFilterContext context)
    {
        // User must be logged in
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult(
                "Login",
                "Account",
                null);

            return;
        }

        // Get logged-in user's ID
        var userIdClaim = context.HttpContext.User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new RedirectToActionResult(
                "AccessDenied",
                "Account",
                null);

            return;
        }

        // Check permission
        bool hasPermission =
            await _permissionService.HasPermissionAsync(
                userId,
                _permission);

        if (!hasPermission)
        {
            context.Result = new RedirectToActionResult(
                "AccessDenied",
                "Account",
                null);

            return;
        }
    }
}