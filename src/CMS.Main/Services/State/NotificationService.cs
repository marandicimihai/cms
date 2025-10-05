using CMS.Main.Abstractions.Notifications;

namespace CMS.Main.Services.State;

public class NotificationService : INotificationService
{
    public event Action<Notification>? OnNotify;

    public Task NotifyAsync(Notification notification)
    {
        OnNotify?.Invoke(notification);
        return Task.CompletedTask;
    }
}
