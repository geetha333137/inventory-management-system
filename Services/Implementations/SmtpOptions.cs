using MailKit.Security;

namespace InventoryManagementSystem.Services;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "";

    public int Port { get; set; } = 587;

    public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.StartTls;

    public string UserName { get; set; } = "";

    public string Password { get; set; } = "";

    public string FromEmail { get; set; } = "";

    public string FromName { get; set; } = "Inventory Management System";

    public string DefaultRecipientEmail { get; set; } = "";
}
