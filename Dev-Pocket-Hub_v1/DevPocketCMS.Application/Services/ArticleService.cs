using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Core.Models;

namespace DevPocketCMS.Application.Services;

public class ArticleService(IArticleRepository articles, IListRepository lists, ITagRepository tags,
    ILinkPreviewService preview, IPocketModeService pocket)
{
    private readonly IArticleRepository _articles = articles;
    private readonly IListRepository _lists = lists;
    private readonly ILinkPreviewService _preview = preview;

    public async Task<List<ArticleDto>> GetAllAsync()
    {
        var articles = await _articles.GetAllWithTagsAsync();
        var lists = await _lists.GetAllAsync();
        return [.. articles.Select(a => ToDto(a, lists))];
    }

    public async Task<List<ArticleDto>> GetByListAsync(int listId)
    {
        var articles = await _articles.GetByListAsync(listId);
        var lists = await _lists.GetAllAsync();

        // Hydrate tags
        foreach (var a in articles)
            a.Tags = await tags.GetTagsForArticleAsync(a.Id);

        return [.. articles.Select(a => ToDto(a, lists))];
    }

    /// <summary>
    /// Saves a new article, optionally fetching a preview and/or saving HTML (pocket mode).
    /// </summary>
    public async Task<ArticleDto> SaveAsync(string url, int listId, List<int> tagIds, string? manualTitle = null,
        bool fetchPreview = true, bool enablePocketMode = false)
    {
        var article = new Article
        {
            Url = url.Trim(),
            ListId = listId,
            SavedAt = DateTime.UtcNow,
        };

        if (fetchPreview)
        {
            var preview = await _preview.FetchPreviewAsync(url);
            article.Title = manualTitle ?? preview.Title;
            article.Description = preview.Description;
            article.ImageUrl = preview.ImageUrl;
            article.Favicon = preview.Favicon;
            article.SiteName = preview.SiteName;
        }
        else
        {
            article.Title = manualTitle ?? url;
        }

        var id = await _articles.InsertAsync(article);
        article.Id = id;

        if (tagIds.Any())
            await tags.SetTagsForArticleAsync(id, tagIds);

        if (enablePocketMode)
        {
            var contentPath = await pocket.SavePageAsync(url, id);
            article.PocketContentPath = contentPath;
            await _articles.UpdateAsync(article);
        }

        var lists = await _lists.GetAllAsync();
        article.Tags = await tags.GetTagsForArticleAsync(id);
        return ToDto(article, lists);
    }

    public async Task MarkReadAsync(int id, bool isRead)
    {
        var article = await _articles.GetByIdAsync(id);
        if (article is null) return;

        article.IsRead = isRead;
        article.ReadAt = isRead ? DateTime.UtcNow : null;
        await _articles.UpdateAsync(article);
    }

    public async Task ToggleFavoriteAsync(int id)
    {
        var article = await _articles.GetByIdAsync(id);
        if (article is null) return;

        article.IsFavorite = !article.IsFavorite;
        await _articles.UpdateAsync(article);
    }

    public async Task SaveNotesAsync(int id, string notes)
    {
        var article = await _articles.GetByIdAsync(id);
        if (article is null) return;

        article.Notes = notes;
        await _articles.UpdateAsync(article);
    }

    public async Task UpdateTagsAsync(int articleId, List<int> tagIds)
    {
        await tags.SetTagsForArticleAsync(articleId, tagIds);
    }

    public async Task DeleteAsync(int id)
    {
        var article = await _articles.GetByIdAsync(id);
        if (article?.PocketContentPath is not null)
            await pocket.DeletePageAsync(article.PocketContentPath);

        await _articles.DeleteAsync(id);
    }

    public Task<int> GetUnreadCountAsync() => _articles.GetUnreadCountAsync();

    // ── mapping ──────────────────────────────────────────────────────────────

    private static ArticleDto ToDto(Article a, List<Core.Entities.ArticleList> lists)
    {
        var list = lists.FirstOrDefault(l => l.Id == a.ListId);
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
            Tags = [.. a.Tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
            })],
        };
    }
}
