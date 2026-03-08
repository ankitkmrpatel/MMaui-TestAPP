using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;

namespace DevPocketCMS.Application.Services;

public class ListService(IListRepository lists, IArticleRepository articles)
{
    private readonly IListRepository _lists = lists;
    private readonly IArticleRepository _articles = articles;

    public async Task<List<ListDto>> GetAllAsync()
    {
        var lists = await _lists.GetAllAsync();
        var articles = await _articles.GetAllAsync();

        return [.. lists.Select(l => new ListDto
        {
            Id           = l.Id,
            Name         = l.Name,
            Color        = l.Color,
            Icon         = l.Icon,
            ArticleCount = articles.Count(a => a.ListId == l.Id),
            UnreadCount  = articles.Count(a => a.ListId == l.Id && !a.IsRead),
        })];
    }

    public async Task<ListDto> CreateAsync(string name, string color, string icon)
    {
        var list = new ArticleList
        {
            Name = name,
            Color = color,
            Icon = icon,
            CreatedAt = DateTime.UtcNow,
        };

        var id = await _lists.InsertAsync(list);
        list.Id = id;

        return new ListDto { Id = id, Name = name, Color = color, Icon = icon };
    }

    public async Task UpdateAsync(int id, string name, string color, string icon)
    {
        var list = await _lists.GetByIdAsync(id);
        if (list is null) return;

        list.Name = name;
        list.Color = color;
        list.Icon = icon;
        await _lists.UpdateAsync(list);
    }

    public Task DeleteAsync(int id) => _lists.DeleteAsync(id);
}
