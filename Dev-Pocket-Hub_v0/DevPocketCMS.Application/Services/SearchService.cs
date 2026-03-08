using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Core.Models;

namespace DevPocketCMS.Application.Services;

/// <summary>
/// Advanced search supporting AND / OR logic across text, tags, and lists.
/// </summary>
public class SearchService(IArticleRepository articles, IListRepository lists)
{
    private readonly IArticleRepository _articles = articles;
    private readonly IListRepository _lists = lists;

    public async Task<List<ArticleDto>> SearchAsync(SearchOptions options)
    {
        var articles = await _articles.GetAllWithTagsAsync();
        var lists = await _lists.GetAllAsync();
        var listDict = lists.ToDictionary(l => l.Id);

        var terms = (options.Query ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IEnumerable<Core.Entities.Article> result = articles;

        if (options.Logic == SearchLogic.AND)
        {
            if (terms.Any())
                result = result.Where(a => terms.All(t => MatchText(a, t)));

            if (options.TagIds.Any())
                result = result.Where(a =>
                    options.TagIds.All(tid => a.Tags.Any(t => t.Id == tid)));

            if (options.ListIds.Any())
                result = result.Where(a => options.ListIds.Contains(a.ListId));
        }
        else // OR
        {
            bool hasFilters = terms.Any() || options.TagIds.Any() || options.ListIds.Any();
            if (hasFilters)
            {
                result = result.Where(a =>
                    terms.Any(t => MatchText(a, t)) ||
                    options.TagIds.Any(tid => a.Tags.Any(t => t.Id == tid)) ||
                    options.ListIds.Contains(a.ListId));
            }
        }

        if (options.IsRead.HasValue)
            result = result.Where(a => a.IsRead == options.IsRead.Value);

        if (options.IsFavorite.HasValue)
            result = result.Where(a => a.IsFavorite == options.IsFavorite.Value);

        return result
            .OrderByDescending(a => a.SavedAtTicks)
            .Select(a =>
            {
                listDict.TryGetValue(a.ListId, out var list);
                return new ArticleDto
                {
                    Id = a.Id,
                    Url = a.Url,
                    Title = a.Title,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    Favicon = a.Favicon,
                    SiteName = a.SiteName,
                    ListId = a.ListId,
                    ListName = list?.Name ?? "—",
                    ListColor = list?.Color ?? "#4FFFB0",
                    IsRead = a.IsRead,
                    IsFavorite = a.IsFavorite,
                    HasPocketContent = !string.IsNullOrEmpty(a.PocketContentPath),
                    Notes = a.Notes,
                    SavedAt = a.SavedAt,
                    ReadAt = a.ReadAt,
                    Tags = a.Tags.Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Color = t.Color,
                    }).ToList(),
                };
            })
            .ToList();
    }

    private static bool MatchText(Core.Entities.Article a, string term) =>
        a.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
        (a.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (a.SiteName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (a.Notes?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
        a.Tags.Any(t => t.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
}
