using DevPocketCMS.Application.DTOs;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;
using Microsoft.Maui.ApplicationModel;

namespace DevPocketCMS.UI.Components;

/// <summary>
/// Swipeable article card. Tap the card body to open the URL in the browser.
/// Swipe left → toggle read. Swipe right → delete.
/// </summary>
class ArticleCard : Component
{
    private ArticleDto? _article;
    private Action<int>? _onToggleRead;
    private Action<int>? _onToggleFavorite;
    private Action<int>? _onDelete;

    public ArticleCard Article(ArticleDto article) { _article = article; return this; }
    public ArticleCard OnToggleRead(Action<int> cb) { _onToggleRead = cb; return this; }
    public ArticleCard OnToggleFavorite(Action<int> cb) { _onToggleFavorite = cb; return this; }
    public ArticleCard OnDelete(Action<int> cb) { _onDelete = cb; return this; }

    public override VisualNode Render()
    {
        if (_article is null) return VStack();

        var a = _article;

        return SwipeView(
            // ── Card body ─────────────────────────────────────────────────
            Border(
                Grid(rows: "Auto, Auto, Auto", columns: "*, Auto",

                    // Title
                    Label(a.Title)
                        .FontSize(15)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(a.IsRead ? AppColors.TextMuted : AppColors.TextPrimary)
                        .LineBreakMode(LineBreakMode.TailTruncation)
                        .MaxLines(2),

                    // Favorite star
                    Label(a.IsFavorite ? "⭐" : "☆")
                        .FontSize(18)
                        .GridColumn(1)
                        .VStart()
                        .OnTapped(() => _onToggleFavorite?.Invoke(a.Id)),

                    // Description
                    !string.IsNullOrWhiteSpace(a.Description)
                        ? Label(a.Description)
                            .FontSize(12)
                            .TextColor(AppColors.TextSecondary)
                            .LineBreakMode(LineBreakMode.TailTruncation)
                            .MaxLines(2)
                            .GridRow(1)
                            .GridColumnSpan(2)
                            .Margin(0, 4, 0, 0)
                        : null,

                    // Meta row
                    HStack(
                        // List badge
                        Border(
                            Label(a.ListName).FontSize(10).TextColor(Color.FromArgb(a.ListColor)).Padding(6, 3)
                        )
                        .StrokeShape(new RoundRectangle().CornerRadius(6))
                        .BackgroundColor(Color.FromRgba(
                            Color.FromArgb(a.ListColor).Red,
                            Color.FromArgb(a.ListColor).Green,
                            Color.FromArgb(a.ListColor).Blue,
                            0.15f))
                        .Stroke(Color.FromArgb(a.ListColor))
                        .StrokeThickness(0.5f),

                        // Tags
                        HStack(
                            a.Tags.Take(3).Select(t => (VisualNode)
                                Border(Label(t.Name).FontSize(10).TextColor(AppColors.TextMuted).Padding(5, 2))
                                    .StrokeShape(new RoundRectangle().CornerRadius(5))
                                    .BackgroundColor(AppColors.TagBg)
                                    .Stroke(Colors.Transparent)
                            ).ToArray()
                        )
                        .Spacing(4),

                        // Pocket badge
                        a.HasPocketContent
                            ? Border(Label("📥 Offline").FontSize(10).TextColor(AppColors.Accent).Padding(5, 2))
                                .StrokeShape(new RoundRectangle().CornerRadius(5))
                                .BackgroundColor(AppColors.AccentBg)
                                .Stroke(AppColors.Accent)
                                .StrokeThickness(0.5f)
                            : null,

                        // Time (pushes to right)
                        Label(a.SavedAgo)
                            .FontSize(11)
                            .TextColor(AppColors.TextMuted)
                            .HEnd()
                            .HorizontalOptions(LayoutOptions.EndAndExpand)
                    )
                    .Spacing(5)
                    .GridRow(2)
                    .GridColumnSpan(2)
                    .Margin(0, 6, 0, 0)
                )
                .Padding(16, 14)
            )
            .StrokeShape(new RoundRectangle().CornerRadius(14))
            .BackgroundColor(a.IsRead ? AppColors.Surface : AppColors.Card)
            .Stroke(a.IsRead ? AppColors.CardBorder : Color.FromArgb("#2A2A48"))
            .StrokeThickness(1)
            .Margin(20, 5)
            // ── Tap card to open URL in browser ──────────────────────────
            .OnTapped(async () =>
            {
                if (!string.IsNullOrWhiteSpace(a.Url) &&
                    Uri.TryCreate(a.Url, UriKind.Absolute, out var uri))
                {
                    await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                }
            })
        )
        // Swipe right → Delete
        .RightItems(SwipeItems(
            SwipeItem()
                .Text("Delete")
                .BackgroundColor(AppColors.Danger)
                .OnInvoked(() => _onDelete?.Invoke(a.Id))
            )
        )
        // Swipe left → Toggle Read
        .LeftItems(SwipeItems(
            SwipeItem()
                .Text(a.IsRead ? "Unread" : "Read")
                .BackgroundColor(AppColors.AccentDark)
                .OnInvoked(() => _onToggleRead?.Invoke(a.Id))
            )
        );
    }
}
