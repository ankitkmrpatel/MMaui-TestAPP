namespace DevPocketCMS.Core.Interfaces;

public interface IPocketModeService
{
    /// <summary>Downloads and saves the full HTML content of the URL locally.
    /// Returns the local file path where the content is stored.</summary>
    Task<string> SavePageAsync(string url, int articleId);

    /// <summary>Reads the locally saved HTML content for an article.</summary>
    Task<string?> LoadPageAsync(string contentPath);

    /// <summary>Deletes the locally saved HTML file for an article.</summary>
    Task DeletePageAsync(string contentPath);
}
