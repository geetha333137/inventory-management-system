namespace InventoryManagementSystem.Services;

public interface IEmailService
{
    Task SendNotificationAsync(
        string recipientEmail,
        string recipientName,
        string notificationTitle,
        string notificationMessage,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
