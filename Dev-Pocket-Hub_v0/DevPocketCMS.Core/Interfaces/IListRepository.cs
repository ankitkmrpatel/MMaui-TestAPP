using DevPocketCMS.Core.Entities;

namespace DevPocketCMS.Core.Interfaces;

public interface IListRepository
{
    Task<List<ArticleList>> GetAllAsync();
    Task<ArticleList?> GetByIdAsync(int id);
    Task<int> InsertAsync(ArticleList list);
    Task UpdateAsync(ArticleList list);
    Task DeleteAsync(int id);
}
