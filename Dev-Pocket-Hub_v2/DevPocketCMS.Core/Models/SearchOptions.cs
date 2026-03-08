namespace DevPocketCMS.Core.Models;

public enum SearchLogic { OR, AND }

public class SearchOptions
{
    public string Query { get; set; } = string.Empty;
    public SearchLogic Logic { get; set; } = SearchLogic.OR;
    public List<int> TagIds { get; set; } = [];
    public List<int> ListIds { get; set; } = [];
    public bool? IsRead { get; set; }
    public bool? IsFavorite { get; set; }
}
