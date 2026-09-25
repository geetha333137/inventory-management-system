namespace NotificationButtonOnly_MVC.Models;

// JSON sent from the browser to NotificationsController.Create().
public class NotificationRequest
{
    public string Type { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string AssetCode { get; set; } = "";
}
