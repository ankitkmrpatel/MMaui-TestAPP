using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;

namespace DevPocketCMS.Infrastructure.Repositories;

public class ListRepository : IListRepository
{
    private readonly AppDatabase _database;

    public ListRepository(AppDatabase database) => _database = database;

    public async Task<List<ArticleList>> GetAllAsync()
    {
        var lists = await _database.Db.Table<ArticleList>().ToListAsync();
        var allArticles = await _database.Db.Table<Article>().ToListAsync();

        foreach (var list in lists)
            list.ArticleCount = allArticles.Count(a => a.ListId == list.Id);

        return lists;
    }

    public Task<ArticleList?> GetByIdAsync(int id) =>
        _database.Db.Table<ArticleList>().Where(l => l.Id == id).FirstOrDefaultAsync()!;

    public Task<int> InsertAsync(ArticleList list) =>
        _database.Db.InsertAsync(list);

    public Task UpdateAsync(ArticleList list) =>
        _database.Db.UpdateAsync(list);

    public async Task DeleteAsync(int id)
    {
        await _database.Db.DeleteAsync<ArticleList>(id);
        // Reassign orphaned articles to list 1 (Reading List)
        await _database.Db.ExecuteAsync(
            "UPDATE Articles SET ListId = 1 WHERE ListId = ?", id);
    }
}
