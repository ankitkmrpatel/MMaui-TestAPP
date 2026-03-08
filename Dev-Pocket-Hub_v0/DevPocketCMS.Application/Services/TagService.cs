using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;

namespace DevPocketCMS.Application.Services;

public class TagService(ITagRepository tags)
{
    private readonly ITagRepository _tags = tags;

    public async Task<List<TagDto>> GetAllAsync()
    {
        var tags = await _tags.GetAllAsync();
        return tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Color = t.Color,
        }).ToList();
    }

    public async Task<TagDto> GetOrCreateAsync(string name, string color = "#8888BB")
    {
        var existing = await _tags.GetByNameAsync(name);
        if (existing is not null)
            return new TagDto { Id = existing.Id, Name = existing.Name, Color = existing.Color };

        var tag = new Tag { Name = name.Trim(), Color = color };
        var id = await _tags.InsertAsync(tag);
        return new TagDto { Id = id, Name = name, Color = color };
    }

    public Task DeleteAsync(int id) => _tags.DeleteAsync(id);
}
