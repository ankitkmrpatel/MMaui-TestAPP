using DevPocketCMS.Core.Interfaces;

namespace DevPocketCMS.Infrastructure.Services;

/// <summary>
/// Saves full HTML of a web page to the device's local storage
/// so it can be read offline (Pocket mode).
/// </summary>
public class PocketModeService : IPocketModeService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
        DefaultRequestHeaders =
        {
            { "User-Agent", "DevPocketCMS/1.0 (pocket-mode)" }
        }
    };

    private static string PocketDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "pocket_pages");

    public async Task<string> SavePageAsync(string url, int articleId)
    {
        Directory.CreateDirectory(PocketDir);

        var html = await _http.GetStringAsync(url);

        var filePath = Path.Combine(PocketDir, $"article_{articleId}.html");
        await File.WriteAllTextAsync(filePath, html);

        return filePath;
    }

    public async Task<string?> LoadPageAsync(string contentPath)
    {
        if (string.IsNullOrWhiteSpace(contentPath) || !File.Exists(contentPath))
            return null;

        return await File.ReadAllTextAsync(contentPath);
    }

    public Task DeletePageAsync(string contentPath)
    {
        if (!string.IsNullOrWhiteSpace(contentPath) && File.Exists(contentPath))
            File.Delete(contentPath);

        return Task.CompletedTask;
    }
}
