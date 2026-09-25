namespace NotificationButtonOnly_MVC.Models;

// One notification record shown in the dropdown.
public class Notification
{
    public long Id { get; set; }

    // ADD, UPDATE, or DELETE.
    public string Type { get; set; } = "";

    // Dropdown heading.
    public string Title { get; set; } = "";

    // Dropdown message.
    public string Message { get; set; } = "";

    // Asset information copied from the asset operation.
    public string AssetName { get; set; } = "";
    public string AssetCode { get; set; } = "";

    // false = unread, true = read.
    public bool IsRead { get; set; }

    // Time shown in the dropdown.
    public DateTime CreatedAt { get; set; }
}
