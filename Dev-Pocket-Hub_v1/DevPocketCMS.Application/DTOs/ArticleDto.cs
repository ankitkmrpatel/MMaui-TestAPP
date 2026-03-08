namespace DevPocketCMS.Application.DTOs;

public class ArticleDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Favicon { get; set; }
    public string? SiteName { get; set; }
    public int ListId { get; set; }
    public string ListName { get; set; } = string.Empty;
    public string ListColor { get; set; } = "#4FFFB0";
    public bool IsRead { get; set; }
    public bool IsFavorite { get; set; }
    public bool HasPocketContent { get; set; }
    public string? Notes { get; set; }
    public DateTime SavedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public List<TagDto> Tags { get; set; } = new();

    public string SavedAgo =>
        (DateTime.UtcNow - SavedAt) switch
        {
            { TotalMinutes: < 1 } => "just now",
            { TotalHours: < 1 } => $"{(int)(DateTime.UtcNow - SavedAt).TotalMinutes}m ago",
            { TotalDays: < 1 } => $"{(int)(DateTime.UtcNow - SavedAt).TotalHours}h ago",
            { TotalDays: < 30 } => $"{(int)(DateTime.UtcNow - SavedAt).TotalDays}d ago",
            _ => SavedAt.ToString("MMM yyyy"),
        };
}
