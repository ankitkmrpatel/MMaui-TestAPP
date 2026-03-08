using SQLite;

namespace DevPocketCMS.Core.Entities;

[Table("Lists")]
public class ArticleList
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    /// <summary>Hex color, e.g. "#4FFFB0"</summary>
    public string Color { get; set; } = "#4FFFB0";

    /// <summary>Material icon name used in UI (e.g. "bookmark")</summary>
    public string Icon { get; set; } = "bookmark";

    public long CreatedAtTicks { get; set; } = DateTime.UtcNow.Ticks;

    [Ignore]
    public DateTime CreatedAt
    {
        get => new DateTime(CreatedAtTicks, DateTimeKind.Utc);
        set => CreatedAtTicks = value.Ticks;
    }

    [Ignore]
    public int ArticleCount { get; set; }
}
