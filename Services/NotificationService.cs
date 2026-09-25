using System.Text.Json;
using InventoryManagementSystem.Services;
using Microsoft.Extensions.Options;
using NotificationButtonOnly_MVC.Models;

namespace NotificationButtonOnly_MVC.Services;

// Standalone storage for the notification module.
// Data is saved in notifications.json.
// Your teammate can replace this class with their real database implementation.
public class NotificationService : INotificationService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IEmailService _emailService;
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<NotificationService> _logger;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private long _nextId = 1;

    public NotificationService(
        IWebHostEnvironment environment,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        ILogger<NotificationService> logger)
    {
        _filePath = Path.Combine(
            environment.ContentRootPath,
            "notifications.json");

        _emailService = emailService;
        _smtpOptions = smtpOptions.Value;
        _logger = logger;

        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    public async Task<IReadOnlyList<Notification>> GetAllAsync()
    {
        await _lock.WaitAsync();

        try
        {
            var list = await ReadAsync();

            return list
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    // Creates notification text for ADD, UPDATE, or DELETE.
    public async Task<Notification> CreateAsync(
        string type,
        string assetName,
        string assetCode)
    {
        type = type.Trim().ToUpperInvariant();
        assetName = assetName.Trim();
        assetCode = assetCode.Trim();

        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException("Asset name is required.");

        if (string.IsNullOrWhiteSpace(assetCode))
            throw new ArgumentException("Asset code is required.");

        var content = type switch
        {
            "ADD" => (
                "New Asset Added",
                $"{assetName} ({assetCode}) has been added to inventory."
            ),

            "UPDATE" => (
                "Asset Updated",
                $"{assetName} ({assetCode}) has been updated."
            ),

            "DELETE" => (
                "Asset Deleted",
                $"{assetName} ({assetCode}) has been deleted from inventory."
            ),

            _ => throw new ArgumentException(
                "Type must be ADD, UPDATE or DELETE.")
        };

        await _lock.WaitAsync();

        try
        {
            var list = await ReadAsync();

            var notification = new Notification
            {
                Id = _nextId++,
                Type = type,
                Title = content.Item1,
                Message = content.Item2,
                AssetName = assetName,
                AssetCode = assetCode,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            list.Insert(0, notification);

            await WriteAsync(list);

            await SendEmailAsync(notification);

            return notification;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Mark one notification as read.
    public async Task<bool> MarkAsReadAsync(long id)
    {
        await _lock.WaitAsync();

        try
        {
            var list = await ReadAsync();

            var item = list.FirstOrDefault(x => x.Id == id);

            if (item is null)
                return false;

            item.IsRead = true;

            await WriteAsync(list);

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Mark all notifications as read.
    public async Task MarkAllAsReadAsync()
    {
        await _lock.WaitAsync();

        try
        {
            var list = await ReadAsync();

            foreach (var item in list)
                item.IsRead = true;

            await WriteAsync(list);
        }
        finally
        {
            _lock.Release();
        }
    }

    // Delete one notification.
    public async Task<bool> DeleteAsync(long id)
    {
        await _lock.WaitAsync();

        try
        {
            var list = await ReadAsync();

            var item = list.FirstOrDefault(x => x.Id == id);

            if (item is null)
                return false;

            list.Remove(item);

            await WriteAsync(list);

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Delete all notifications.
    public async Task ClearAllAsync()
    {
        await _lock.WaitAsync();

        try
        {
            await WriteAsync(new List<Notification>());
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Notification>> ReadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<Notification>();

        var json = await File.ReadAllTextAsync(_filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<Notification>();

        var list =
            JsonSerializer.Deserialize<List<Notification>>(
                json,
                _jsonOptions)
            ?? new List<Notification>();

        if (list.Count > 0)
        {
            _nextId =
                Math.Max(
                    _nextId,
                    list.Max(x => x.Id) + 1);
        }

        return list;
    }

    private async Task WriteAsync(List<Notification> list)
    {
        var json =
            JsonSerializer.Serialize(
                list,
                _jsonOptions);

        await File.WriteAllTextAsync(
            _filePath,
            json);
    }

    private async Task SendEmailAsync(Notification notification)
    {
        if (string.IsNullOrWhiteSpace(_smtpOptions.DefaultRecipientEmail))
        {
            return;
        }

        try
        {
            await _emailService.SendNotificationAsync(
                _smtpOptions.DefaultRecipientEmail,
                "StockSphere user",
                notification.Title,
                notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unable to send notification email to {RecipientEmail}.",
                _smtpOptions.DefaultRecipientEmail);
        }
    }
}
