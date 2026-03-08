using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;

namespace DevPocketCMS.Infrastructure.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly AppDatabase _database;

    public ArticleRepository(AppDatabase database) => _database = database;

    public Task<List<Article>> GetAllAsync() =>
        _database.Db.Table<Article>().OrderByDescending(a => a.SavedAtTicks).ToListAsync();

    public Task<List<Article>> GetByListAsync(int listId) =>
        _database.Db.Table<Article>()
            .Where(a => a.ListId == listId)
            .OrderByDescending(a => a.SavedAtTicks)
            .ToListAsync();

    public Task<Article?> GetByIdAsync(int id) =>
        _database.Db.Table<Article>().Where(a => a.Id == id).FirstOrDefaultAsync()!;

    public async Task<List<Article>> GetAllWithTagsAsync()
    {
        var articles = await GetAllAsync();
        var allArticleTags = await _database.Db.Table<ArticleTag>().ToListAsync();
        var allTags = await _database.Db.Table<Tag>().ToListAsync();
        var tagDict = allTags.ToDictionary(t => t.Id);

        foreach (var article in articles)
        {
            var tagIds = allArticleTags
                .Where(at => at.ArticleId == article.Id)
                .Select(at => at.TagId);

            article.Tags = tagIds
                .Where(tagDict.ContainsKey)
                .Select(id => tagDict[id])
                .ToList();
        }

        return articles;
    }

    public Task<int> InsertAsync(Article article) =>
        _database.Db.InsertAsync(article);

    public Task UpdateAsync(Article article) =>
        _database.Db.UpdateAsync(article);

    public async Task DeleteAsync(int id)
    {
        await _database.Db.DeleteAsync<Article>(id);
        await _database.Db.ExecuteAsync(
            "DELETE FROM ArticleTags WHERE ArticleId = ?", id);
    }

    public async Task<int> GetUnreadCountAsync() =>
        await _database.Db.Table<Article>().Where(a => !a.IsRead).CountAsync();
}
