using DevPocketCMS.Core.Entities;
using SQLite;

namespace DevPocketCMS.Infrastructure.Data;

/// <summary>
/// Singleton wrapper around SQLiteAsyncConnection.
/// Call InitializeAsync() once at app startup (MauiProgram).
/// </summary>
public class AppDatabase
{
    private const string DbName = "devpocketcms.db3";

    private static readonly SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private readonly SQLiteAsyncConnection _db;

    public AppDatabase()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbName);

        _db = new SQLiteAsyncConnection(dbPath, Flags);
    }

    public SQLiteAsyncConnection Db => _db;

    public async Task InitializeAsync()
    {
        System.Diagnostics.Debug.WriteLine("Starting With Migration");

        try
        {
            await _db.CreateTableAsync<Article>();
            await _db.CreateTableAsync<ArticleList>();
            await _db.CreateTableAsync<Tag>();
            await _db.CreateTableAsync<ArticleTag>();
            await _db.CreateTableAsync<UserPreferences>();

            await SeedDefaultDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Exception Encountered.");

            System.Diagnostics.Debug.WriteLine("Exception Happen #{0}", ex);
            throw;
        }
    }

    private async Task SeedDefaultDataAsync()
    {
        var listCount = await _db.Table<ArticleList>().CountAsync();
        if (listCount == 0)
        {
            await _db.InsertAllAsync(new[]
            {
                new ArticleList { Name = "Reading List",  Color = "#4FFFB0", Icon = "bookmark" },
                new ArticleList { Name = "Favorites",     Color = "#FFD166", Icon = "star" },
                new ArticleList { Name = "Archive",       Color = "#8888BB", Icon = "archive" },
            });
        }

        var tagCount = await _db.Table<Tag>().CountAsync();
        if (tagCount == 0)
        {
            await _db.InsertAllAsync(new[]
            {
                new Tag { Name = ".NET",        Color = "#A78BFA" },
                new Tag { Name = "MAUI",        Color = "#4FFFB0" },
                new Tag { Name = "Architecture",Color = "#00C2FF" },
                new Tag { Name = "DevOps",      Color = "#FF6B6B" },
                new Tag { Name = "LinkedIn",    Color = "#0B7FBB" },
                new Tag { Name = "Medium",      Color = "#FFD166" },
            });
        }
    }
}