using SQLite;

namespace DevPocketCMS.Core.Entities;

/// <summary>Junction table for the many-to-many Article ↔ Tag relationship.</summary>
[Table("ArticleTags")]
public class ArticleTag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ArticleId { get; set; }

    [Indexed]
    public int TagId { get; set; }
}
