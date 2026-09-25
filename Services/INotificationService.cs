using NotificationButtonOnly_MVC.Models;

namespace NotificationButtonOnly_MVC.Services;

// Keeps notification business logic independent from the controller.
public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetAllAsync();

    Task<Notification> CreateAsync(
        string type,
        string assetName,
        string assetCode);

    Task<bool> MarkAsReadAsync(long id);

    Task MarkAllAsReadAsync();

    Task<bool> DeleteAsync(long id);

    Task ClearAllAsync();
}
