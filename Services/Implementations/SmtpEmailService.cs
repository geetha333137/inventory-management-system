using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace InventoryManagementSystem.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = "Reset your Inventory Management password";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = $"Hello {recipientName},\n\n" +
                       "Use the link below to reset your Inventory Management password. " +
                       "This link expires in 30 minutes.\n\n" +
                       resetUrl + "\n\n" +
                       "If you did not request this, you can safely ignore this email.",
            HtmlBody = $"<p>Hello {System.Net.WebUtility.HtmlEncode(recipientName)},</p>" +
                       "<p>Use the button below to reset your Inventory Management password. " +
                       "This link expires in <strong>30 minutes</strong>.</p>" +
                       $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">Reset password</a></p>" +
                       "<p>If you did not request this, you can safely ignore this email.</p>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            _options.SecureSocketOptions,
            cancellationToken);

        await smtpClient.AuthenticateAsync(
            _options.UserName,
            _options.Password,
            cancellationToken);

        await smtpClient.SendAsync(message, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation(
            "Password reset email sent to {RecipientEmail}.",
            recipientEmail);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.UserName) ||
            string.IsNullOrWhiteSpace(_options.Password) ||
            string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException(
                "SMTP is not configured. Set Smtp:Host, Smtp:UserName, " +
                "Smtp:Password, and Smtp:FromEmail.");
        }

        if (_options.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("Smtp:Port must be between 1 and 65535.");
        }
    }
}
