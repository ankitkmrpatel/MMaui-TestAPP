using DevPocketCMS.Core.Entities;

namespace DevPocketCMS.Core.Interfaces;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<Tag?> GetByNameAsync(string name);
    Task<int> InsertAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(int id);
    Task<List<Tag>> GetTagsForArticleAsync(int articleId);
    Task SetTagsForArticleAsync(int articleId, IEnumerable<int> tagIds);
}
