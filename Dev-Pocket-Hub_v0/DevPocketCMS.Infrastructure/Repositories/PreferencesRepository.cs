using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;

namespace DevPocketCMS.Infrastructure.Repositories;

public class PreferencesRepository : IPreferencesRepository
{
    private readonly AppDatabase _database;

    public PreferencesRepository(AppDatabase database) => _database = database;

    public async Task<UserPreferences> GetAsync()
    {
        var prefs = await _database.Db.Table<UserPreferences>()
            .FirstOrDefaultAsync();

        if (prefs is null)
        {
            prefs = new UserPreferences();
            await _database.Db.InsertAsync(prefs);
        }

        return prefs;
    }

    public Task SaveAsync(UserPreferences preferences) =>
        _database.Db.InsertOrReplaceAsync(preferences);
}
