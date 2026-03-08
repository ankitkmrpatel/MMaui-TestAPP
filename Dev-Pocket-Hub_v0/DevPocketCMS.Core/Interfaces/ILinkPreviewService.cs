using DevPocketCMS.Core.Models;

namespace DevPocketCMS.Core.Interfaces;

public interface ILinkPreviewService
{
    /// <summary>Fetch Open Graph / meta data from the given URL.</summary>
    Task<LinkPreviewData> FetchPreviewAsync(string url);
}
