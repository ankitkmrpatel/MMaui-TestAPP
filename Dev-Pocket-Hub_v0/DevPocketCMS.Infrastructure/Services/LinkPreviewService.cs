using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Core.Models;
using HtmlAgilityPack;

namespace DevPocketCMS.Infrastructure.Services;

/// <summary>
/// Fetches Open Graph and standard meta tags to build a link preview.
/// Uses HtmlAgilityPack for robust HTML parsing.
/// </summary>
public class LinkPreviewService : ILinkPreviewService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
        DefaultRequestHeaders =
        {
            { "User-Agent", "DevPocketCMS/1.0 (link-preview-fetcher)" }
        }
    };

    public async Task<LinkPreviewData> FetchPreviewAsync(string url)
    {
        var preview = new LinkPreviewData();

        try
        {
            var html = await _http.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            preview.Title = GetOgOrMeta(doc, "og:title", "title") ?? GetTitle(doc);
            preview.Description = GetOgOrMeta(doc, "og:description", "description") ?? string.Empty;
            preview.ImageUrl = GetOgOrMeta(doc, "og:image", null);
            preview.SiteName = GetOgOrMeta(doc, "og:site_name", null) ?? GetDomain(url);
            preview.Favicon = await ResolveFaviconAsync(url, doc);
        }
        catch
        {
            preview.Title = url;
            preview.SiteName = GetDomain(url);
        }

        return preview;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string? GetOgOrMeta(HtmlDocument doc, string ogProp, string? metaName)
    {
        // Open Graph
        var og = doc.DocumentNode
            .SelectSingleNode($"//meta[@property='{ogProp}']")?
            .GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(og)) return og.Trim();

        // Standard meta
        if (metaName is not null)
        {
            var meta = doc.DocumentNode
                .SelectSingleNode($"//meta[@name='{metaName}']")?
                .GetAttributeValue("content", null);
            if (!string.IsNullOrWhiteSpace(meta)) return meta.Trim();
        }

        return null;
    }

    private static string GetTitle(HtmlDocument doc) =>
        doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? string.Empty;

    private static string GetDomain(string url)
    {
        try { return new Uri(url).Host.Replace("www.", ""); }
        catch { return url; }
    }

    private static async Task<string?> ResolveFaviconAsync(string pageUrl, HtmlDocument doc)
    {
        // Try <link rel="icon"> or <link rel="shortcut icon">
        var linkNode = doc.DocumentNode
            .SelectSingleNode("//link[@rel='icon' or @rel='shortcut icon']");

        var href = linkNode?.GetAttributeValue("href", null);
        if (string.IsNullOrWhiteSpace(href))
        {
            // Default favicon path
            var uri = new Uri(pageUrl);
            var defaultFavicon = $"{uri.Scheme}://{uri.Host}/favicon.ico";

            try
            {
                var resp = await _http.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, defaultFavicon));
                return resp.IsSuccessStatusCode ? defaultFavicon : null;
            }
            catch { return null; }
        }

        // Resolve relative paths
        try
        {
            return new Uri(new Uri(pageUrl), href).ToString();
        }
        catch { return href; }
    }
}
