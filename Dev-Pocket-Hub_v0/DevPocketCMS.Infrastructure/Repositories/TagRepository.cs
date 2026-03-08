using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;

namespace DevPocketCMS.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDatabase _database;

    public TagRepository(AppDatabase database) => _database = database;

    public Task<List<Tag>> GetAllAsync() =>
        _database.Db.Table<Tag>().OrderBy(t => t.Name).ToListAsync();

    public Task<Tag?> GetByIdAsync(int id) =>
        _database.Db.Table<Tag>().Where(t => t.Id == id).FirstOrDefaultAsync()!;

    public Task<Tag?> GetByNameAsync(string name) =>
        _database.Db.Table<Tag>()
            .Where(t => t.Name.ToLower() == name.ToLower())
            .FirstOrDefaultAsync()!;

    public Task<int> InsertAsync(Tag tag) =>
        _database.Db.InsertAsync(tag);

    public Task UpdateAsync(Tag tag) =>
        _database.Db.UpdateAsync(tag);

    public async Task DeleteAsync(int id)
    {
        await _database.Db.DeleteAsync<Tag>(id);
        await _database.Db.ExecuteAsync(
            "DELETE FROM ArticleTags WHERE TagId = ?", id);
    }

    public async Task<List<Tag>> GetTagsForArticleAsync(int articleId)
    {
        var articleTags = await _database.Db.Table<ArticleTag>()
            .Where(at => at.ArticleId == articleId)
            .ToListAsync();

        var tagIds = articleTags.Select(at => at.TagId).ToList();
        if (!tagIds.Any()) return new List<Tag>();

        var allTags = await _database.Db.Table<Tag>().ToListAsync();
        return allTags.Where(t => tagIds.Contains(t.Id)).ToList();
    }

    public async Task SetTagsForArticleAsync(int articleId, IEnumerable<int> tagIds)
    {
        await _database.Db.ExecuteAsync(
            "DELETE FROM ArticleTags WHERE ArticleId = ?", articleId);

        var inserts = tagIds.Select(tid => new ArticleTag
        {
            ArticleId = articleId,
            TagId = tid
        });

        await _database.Db.InsertAllAsync(inserts);
    }
}
