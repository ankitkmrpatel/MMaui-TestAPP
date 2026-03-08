namespace DevPocketCMS.Core.Interfaces;

public interface INotificationService
{
    Task<bool> RequestPermissionAsync();
    Task ScheduleDailyReminderAsync(TimeSpan time, int unreadCount);
    Task CancelAllAsync();
}
