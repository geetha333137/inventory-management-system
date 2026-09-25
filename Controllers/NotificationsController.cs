using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationButtonOnly_MVC.Models;
using NotificationButtonOnly_MVC.Services;

namespace NotificationButtonOnly_MVC.Controllers;

// HTTP/API endpoints for the notification module only.
[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _service;

    public NotificationsController(
        INotificationService service)
    {
        _service = service;
    }

    // Displays the page with the single notification button.
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // Returns all notifications and the unread count.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notifications =
            await _service.GetAllAsync();

        return Json(new
        {
            notifications,
            unreadCount =
                notifications.Count(x => !x.IsRead)
        });
    }

    // Creates one in-app notification.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] NotificationRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.Type) ||
            string.IsNullOrWhiteSpace(request.AssetName) ||
            string.IsNullOrWhiteSpace(request.AssetCode))
        {
            return BadRequest(new
            {
                message =
                    "Type, Asset Name and Asset Code are required."
            });
        }

        try
        {
            var notification =
                await _service.CreateAsync(
                    request.Type,
                    request.AssetName,
                    request.AssetCode);

            return Ok(notification);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var success =
            await _service.MarkAsReadAsync(id);

        return success
            ? Ok()
            : NotFound(new
            {
                message =
                    "Notification not found."
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _service.MarkAllAsReadAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var success =
            await _service.DeleteAsync(id);

        return success
            ? Ok()
            : NotFound(new
            {
                message =
                    "Notification not found."
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAll()
    {
        await _service.ClearAllAsync();
        return Ok();
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View("Error");
    }
}
