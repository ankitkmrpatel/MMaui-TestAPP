using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Core.Models;

namespace DevPocketCMS.Application.Services;

public class DashboardService(IArticleRepository articles, IListRepository lists, ITagRepository tags)
{
    private readonly IArticleRepository _articles = articles;
    private readonly IListRepository    _lists    = lists;

    public async Task<DashboardStats> GetStatsAsync()
    {
        var allArticles = await _articles.GetAllWithTagsAsync();
        var allLists    = await _lists.GetAllAsync();
        var allTags     = await tags.GetAllAsync();

        var stats = new DashboardStats
        {
            TotalArticles       = allArticles.Count,
            ReadArticles        = allArticles.Count(a => a.IsRead),
            UnreadArticles      = allArticles.Count(a => !a.IsRead),
            FavoriteArticles    = allArticles.Count(a => a.IsFavorite),
            TotalLists          = allLists.Count,
            TotalTags           = allTags.Count,
            PocketSavedArticles = allArticles.Count(a => !string.IsNullOrEmpty(a.PocketContentPath)),
        };

        // ── Weekly activity (last 7 days) ──────────────────────────────────
        var today = DateTime.Today;
        stats.WeeklyActivity = Enumerable.Range(0, 7).Select(i =>
        {
            var day = today.AddDays(-6 + i);
            int count = allArticles.Count(a => a.SavedAt.ToLocalTime().Date == day);
            return new DayActivity
            {
                DayLabel = day.ToString("ddd"),
                Count    = count,
                IsToday  = day == today,
            };
        }).ToList();

        // ── Reading streak (consecutive days from today going backward) ────
        int streak = 0;
        for (int i = 0; i < 365; i++)
        {
            var day = today.AddDays(-i);
            if (allArticles.Any(a => a.SavedAt.ToLocalTime().Date == day))
                streak++;
            else
                break;
        }
        stats.SavedStreak = streak;

        // ── Per-list stats ─────────────────────────────────────────────────
        stats.ArticlesPerList = [.. allLists
            .Select(l => new ListStat
            {
                ListName  = l.Name,
                ListColor = l.Color,
                Count     = allArticles.Count(a => a.ListId == l.Id),
            })
            .Where(s => s.Count > 0)
            .OrderByDescending(s => s.Count)];

        // ── Top tags ───────────────────────────────────────────────────────
        var tagCounts = new Dictionary<int, int>();
        foreach (var article in allArticles)
            foreach (var tag in article.Tags)
            {
                tagCounts.TryGetValue(tag.Id, out var n);
                tagCounts[tag.Id] = n + 1;
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
