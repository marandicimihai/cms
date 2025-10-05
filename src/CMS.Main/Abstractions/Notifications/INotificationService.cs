namespace CMS.Main.Abstractions.Notifications;

public interface INotificationService
{
    event Action<Notification>? OnNotify;
    Task NotifyAsync(Notification notification);
}
