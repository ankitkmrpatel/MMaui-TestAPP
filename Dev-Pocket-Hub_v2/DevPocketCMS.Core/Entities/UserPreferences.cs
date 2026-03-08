using SQLite;

namespace DevPocketCMS.Core.Entities;

[Table("UserPreferences")]
public class UserPreferences
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    /// <summary>Always "Developer" for now.</summary>
    public string UserType { get; set; } = "Developer";

    /// <summary>light | dark | system</summary>
    public string Theme { get; set; } = "system";

    public bool HasCompletedOnboarding { get; set; }

    public bool NotificationsEnabled { get; set; }

    /// <summary>Daily reminder time in "HH:mm" format, e.g. "09:00"</summary>
    public string? NotificationTime { get; set; } = "09:00";
}