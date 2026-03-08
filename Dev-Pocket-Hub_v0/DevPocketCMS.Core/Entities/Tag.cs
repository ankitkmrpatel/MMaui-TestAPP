using SQLite;

namespace DevPocketCMS.Core.Entities;

[Table("Tags")]
public class Tag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, Unique]
    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#8888BB";
}
