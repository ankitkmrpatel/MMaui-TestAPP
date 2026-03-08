using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Core.Models;

namespace DevPocketCMS.Application.Services;

public class DashboardService(IArticleRepository articles, IListRepository lists, ITagRepository tags)
{
    private readonly IArticleRepository _articles = articles;
    private readonly IListRepository _lists = lists;

    public async Task<DashboardStats> GetStatsAsync()
    {
        var articles = await _articles.GetAllWithTagsAsync();
        var lists = await _lists.GetAllAsync();
        var allTags = await tags.GetAllAsync();

        var stats = new DashboardStats
        {
            TotalArticles = articles.Count,
            ReadArticles = articles.Count(a => a.IsRead),
            UnreadArticles = articles.Count(a => !a.IsRead),
            FavoriteArticles = articles.Count(a => a.IsFavorite),
            TotalLists = lists.Count,
            TotalTags = allTags.Count,
            PocketSavedArticles = articles.Count(a => !string.IsNullOrEmpty(a.PocketContentPath)),
        };

        stats.ArticlesPerList = [.. lists
            .Select(l => new ListStat
            {
                ListName  = l.Name,
                ListColor = l.Color,
                Count     = articles.Count(a => a.ListId == l.Id),
            })
            .Where(s => s.Count > 0)
            .OrderByDescending(s => s.Count)];

        // Count articles per tag
        var tagCounts = new Dictionary<int, int>();
        foreach (var article in articles)
        {
            foreach (var tag in article.Tags)
            {
                tagCounts.TryGetValue(tag.Id, out var count);
                tagCounts[tag.Id] = count + 1;
            }
        }

        var tagDict = allTags.ToDictionary(t => t.Id);
        stats.TopTags = [.. tagCounts
            .OrderByDescending(kv => kv.Value)
            .Take(8)
            .Where(kv => tagDict.ContainsKey(kv.Key))
            .Select(kv => new TagStat
            {
                TagName  = tagDict[kv.Key].Name,
                TagColor = tagDict[kv.Key].Color,
                Count    = kv.Value,
            })];

        return stats;
    }
}
