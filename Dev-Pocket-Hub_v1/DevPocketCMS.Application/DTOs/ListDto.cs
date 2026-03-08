namespace DevPocketCMS.Application.DTOs;

public class ListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#4FFFB0";
    public string Icon { get; set; } = "bookmark";
    public int ArticleCount { get; set; }
    public int UnreadCount { get; set; }
}
