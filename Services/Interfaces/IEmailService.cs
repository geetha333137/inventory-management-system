namespace InventoryManagementSystem.Services;

public interface IEmailService
{
    Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
