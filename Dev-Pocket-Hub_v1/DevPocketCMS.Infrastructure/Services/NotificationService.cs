using DevPocketCMS.Core.Interfaces;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace DevPocketCMS.Infrastructure.Services;

/// <summary>
/// Schedules daily "unread articles" reminders using Plugin.LocalNotification.
/// </summary>
public class NotificationService : Core.Interfaces.INotificationService
{
    private const int DailyReminderId = 1001;

    public async Task<bool> RequestPermissionAsync()
    {
        return await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public async Task ScheduleDailyReminderAsync(TimeSpan time, int unreadCount)
    {
        await CancelAllAsync();

        if (unreadCount == 0) return;

        var notify = time == TimeSpan.Zero
            ? DateTime.Now.AddSeconds(5)
            : DateTime.Today.Add(time);

        if (notify < DateTime.Now)
            notify = notify.AddDays(1);

        var request = new NotificationRequest
        {
            NotificationId = DailyReminderId,
            Title          = "DevPocketCMS",
            Description    = unreadCount == 1
                ? "You have 1 unread article waiting."
                : $"You have {unreadCount} unread articles waiting.",
            BadgeNumber    = unreadCount,
            Schedule       = new NotificationRequestSchedule
            {
                NotifyTime   = notify,
                RepeatType   = NotificationRepeat.Daily,
            },
            Android = new AndroidOptions
            {
                ChannelId = "devpocket_reminders",
                Priority  = AndroidPriority.Default,
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public Task CancelAllAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        return Task.CompletedTask;
    }
}
