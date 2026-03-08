using SQLite;

namespace DevPocketCMS.Core.Entities;

[Table("Articles")]
public class Article
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Url { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Favicon { get; set; }
    public string? SiteName { get; set; }

    public int ListId { get; set; }

    public bool IsRead { get; set; }
    public bool IsFavorite { get; set; }

    /// <summary>Pocket mode: HTML content saved locally as a file path.</summary>
    public string? PocketContentPath { get; set; }

    public string? Notes { get; set; }

    public long SavedAtTicks { get; set; } = DateTime.UtcNow.Ticks;
    public long? ReadAtTicks { get; set; }

    [Ignore]
    public DateTime SavedAt
    {
        get => new DateTime(SavedAtTicks, DateTimeKind.Utc);
        set => SavedAtTicks = value.Ticks;
    }

    [Ignore]
    public DateTime? ReadAt
    {
        get => ReadAtTicks.HasValue ? new DateTime(ReadAtTicks.Value, DateTimeKind.Utc) : null;
        set => ReadAtTicks = value?.Ticks;
    }

    [Ignore]
    public List<Tag> Tags { get; set; } = new();
}
