using DevPocketCMS.Core.Entities;

namespace DevPocketCMS.Core.Interfaces;

public interface IArticleRepository
{
    Task<List<Article>> GetAllAsync();
    Task<List<Article>> GetByListAsync(int listId);
    Task<Article?> GetByIdAsync(int id);
    Task<List<Article>> GetAllWithTagsAsync();
    Task<int> InsertAsync(Article article);
    Task UpdateAsync(Article article);
    Task DeleteAsync(int id);
    Task<int> GetUnreadCountAsync();
}
