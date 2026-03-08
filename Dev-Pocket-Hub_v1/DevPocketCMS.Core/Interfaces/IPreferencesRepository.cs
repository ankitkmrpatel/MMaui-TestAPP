using DevPocketCMS.Core.Entities;

namespace DevPocketCMS.Core.Interfaces;

public interface IPreferencesRepository
{
    Task<UserPreferences> GetAsync();
    Task SaveAsync(UserPreferences preferences);
}
