namespace CMS.Main.Abstractions.Notifications;

public class Notification
{
    public string? Message { get; set; }
    public NotificationType Type { get; set; }
}

public enum NotificationType
{
    Info,
    Warning,
    Success,
    Error
}