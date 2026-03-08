using DevPocketCMS.Application.Services;
using DevPocketCMS.Core.Models;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class DashboardState
{
    public DashboardStats? Stats     { get; set; }
    public bool            IsLoading { get; set; } = true;
}

partial class DashboardPage : Component<DashboardState>
{
    [Inject] readonly DashboardService _dashboard;

    protected override async void OnMounted() => await LoadAsync();

    private async Task LoadAsync()
    {
        SetState(s => s.IsLoading = true);
        var stats = await _dashboard.GetStatsAsync();
        SetState(s => { s.Stats = stats; s.IsLoading = false; });
    }

    // ── Root (no ContentPage — embedded in MainShell) ─────────────────────

    public override VisualNode Render() =>
        ScrollView(
            VStack(
                RenderHeroHeader(),

                State.IsLoading
                    ? ActivityIndicator()
                        .IsRunning(true)
                        .Color(AppColors.Accent)
                        .HCenter()
                        .Margin(0, 48)
                    : State.Stats is null
                        ? null
                        : RenderBody(State.Stats)
            )
            .Spacing(0)
        )
        .BackgroundColor(AppColors.Background);

    // ── Hero header (total + unread) ─────────────────────────────────────

    private VisualNode RenderHeroHeader() =>
        Border(
            Grid(rows: "Auto, Auto, Auto", columns: "*, Auto",

                Label("OVERVIEW")
                    .FontSize(11)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.Accent)
                    .CharacterSpacing(2)
                    .GridRow(0).GridColumnSpan(2),

                Label(State.Stats?.TotalArticles.ToString() ?? "—")
                    .FontSize(56)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextPrimary)
                    .GridRow(1),

                Border(
                    VStack(
                        Label("UNREAD")
                            .FontSize(9)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(AppColors.Accent.WithAlpha(0.7f))
                            .CharacterSpacing(1)
                            .HCenter(),
                        Label((State.Stats?.UnreadArticles ?? 0).ToString())
                            .FontSize(26)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(AppColors.Accent)
                            .HCenter()
                    )
                    .Spacing(2)
                    .Padding(14, 10)
                )
                .StrokeShape(new RoundRectangle().CornerRadius(14))
                .BackgroundColor(AppColors.AccentBg)
                .Stroke(AppColors.Accent.WithAlpha(0.3f))
                .StrokeThickness(1)
                .VCenter()
                .GridRow(1).GridColumn(1),

                Label("total articles saved")
                    .FontSize(13)
                    .TextColor(AppColors.TextMuted)
                    .GridRow(2).GridColumnSpan(2)
                    .Margin(0, -4, 0, 0)
            )
            .Padding(24, 20)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(new CornerRadius(0, 0, 24, 24)))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1);

    // ── Body sections ────────────────────────────────────────────────────

    private VisualNode RenderBody(DashboardStats s) =>
        VStack(
            // Activity chart (weekly bar chart + streak)
            RenderActivitySection(s),

            // Quick stat chips
            RenderQuickStats(s),

            // Reading progress
            s.TotalArticles > 0 ? RenderProgressCard(s) : null,

            // By list breakdown
            s.ArticlesPerList.Any() ? RenderListBreakdown(s) : null,

            // Top tags
            s.TopTags.Any() ? RenderTagCloud(s) : null,

            // Library counts
            RenderLibraryStats(s),

            BoxView().HeightRequest(20).Color(Colors.Transparent)
        )
        .Spacing(0);

    // ── Activity section ─────────────────────────────────────────────────

    private VisualNode RenderActivitySection(DashboardStats s)
    {
        var days   = s.WeeklyActivity;
        int maxCnt = days.Max(d => d.Count);
        // If no data yet, show empty chart with 0 height bars
        double maxBarH = maxCnt > 0 ? 72.0 : 0;

        return Border(
            VStack(

                // Header row: label + streak badge
                HStack(
                    Label("ACTIVITY")
                        .FontSize(11)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.TextSecondary)
                        .CharacterSpacing(1.5)
                        .HFill(),

                    s.SavedStreak > 0
                        ? Border(
                            HStack(
                                Label("🔥").FontSize(13),
                                Label($"{s.SavedStreak} day streak")
                                    .FontSize(12)
                                    .FontAttributes(FontAttributes.Bold)
                                    .TextColor(AppColors.Warning)
                            )
                            .Spacing(4)
                            .Padding(10, 5)
                          )
                          .StrokeShape(new RoundRectangle().CornerRadius(12))
                          .BackgroundColor(Color.FromArgb("#1A1500"))
                          .Stroke(AppColors.Warning.WithAlpha(0.35f))
                          .StrokeThickness(1)
                        : null
                ),

                // Bar chart (7 columns, bottom-aligned)
                Grid(rows: "Auto, Auto, Auto",
                    columns: "*, *, *, *, *, *, *",

                    // Row 0: bars (fixed 80pt chart area, bars grow from bottom)
                    [..days.Select((day, i) =>
                    {
                        double barH = maxCnt > 0
                            ? Math.Max(6, (double)day.Count / maxCnt * maxBarH)
                            : 6;

                        var barColor = day.IsToday
                            ? AppColors.Accent
                            : day.Count > 0
                                ? AppColors.AccentDark
                                : AppColors.Surface;

                        return (VisualNode)Grid(rows: "80", columns: "*",
                            // Subtle track
                            BoxView()
                                .Color(AppColors.Surface)
                                .CornerRadius(4)
                                .WidthRequest(22)
                                .HeightRequest(maxBarH > 0 ? (int)maxBarH : 60)
                                .VEnd()
                                .HCenter(),
                            // Value bar
                            BoxView()
                                .Color(barColor)
                                .CornerRadius(4)
                                .WidthRequest(22)
                                .HeightRequest((int)Math.Max(6, barH))
                                .VEnd()
                                .HCenter()
                        )
                        .GridRow(0)
                        .GridColumn(i);
                    }),

                    // Row 1: day labels
                    ..days.Select((day, i) =>
                        (VisualNode)Label(day.DayLabel)
                            .FontSize(10)
                            .TextColor(day.IsToday ? AppColors.Accent : AppColors.TextMuted)
                            .FontAttributes(day.IsToday ? FontAttributes.Bold : FontAttributes.None)
                            .HCenter()
                            .GridRow(1)
                            .GridColumn(i)
                            .Margin(0, 6, 0, 0)
                    ),

                    // Row 2: counts
                    ..days.Select((day, i) =>
                        (VisualNode)Label(day.Count > 0 ? day.Count.ToString() : "·")
                            .FontSize(10)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(day.Count > 0 ? AppColors.TextSecondary : AppColors.TextMuted.WithAlpha(0.4f))
                            .HCenter()
                            .GridRow(2)
                            .GridColumn(i)
                    )]
                )
                .Margin(0, 14, 0, 0)
            )
            .Spacing(0)
            .Padding(18, 16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(16))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 16, 20, 10);
    }

    // ── Quick stat chips ─────────────────────────────────────────────────

    private VisualNode RenderQuickStats(DashboardStats s) =>
        HStack(
            QuickStatChip("✅", "Read",      s.ReadArticles,      "#4FFFB0"),
            QuickStatChip("⭐", "Favorites", s.FavoriteArticles,  "#FFD166"),
            QuickStatChip("💾", "Offline",   s.PocketSavedArticles,"#44D7B6")
        )
        .Spacing(10)
        .Padding(20, 6, 20, 4);

    private static VisualNode QuickStatChip(string icon, string label, int value, string hex) =>
        Border(
            VStack(
                Label(icon).FontSize(20).HCenter(),
                Label(value.ToString())
                    .FontSize(22)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(Color.FromArgb(hex))
                    .HCenter(),
                Label(label)
                    .FontSize(11)
                    .TextColor(AppColors.TextMuted)
                    .HCenter()
            )
            .Spacing(3)
            .Padding(10, 10)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(Color.FromArgb(hex).WithAlpha(0.2f))
        .StrokeThickness(1)
        .HFill();

    // ── Reading progress ─────────────────────────────────────────────────

    private VisualNode RenderProgressCard(DashboardStats s)
    {
        double pct    = (double)s.ReadArticles / s.TotalArticles;
        int    pctInt = (int)Math.Round(pct * 100);

        return Border(
            VStack(
                HStack(
                    Label("Reading Progress")
                        .FontSize(15)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.TextPrimary)
                        .HFill(),
                    Label($"{pctInt}%")
                        .FontSize(22)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.Accent)
                ),

                ProgressBar()
                    .Progress(pct)
                    .ProgressColor(AppColors.Accent)
                    .BackgroundColor(AppColors.Surface)
                    .HeightRequest(10),

                HStack(
                    Label($"{s.ReadArticles} read")
                        .FontSize(12)
                        .TextColor(AppColors.TextMuted)
                        .HFill(),
                    Label($"{s.UnreadArticles} remaining")
                        .FontSize(12)
                        .TextColor(AppColors.TextMuted)
                )
                .Margin(0, 6, 0, 0)
            )
            .Spacing(10)
            .Padding(18, 16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(16))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.Accent.WithAlpha(0.2f))
        .StrokeThickness(1)
        .Margin(20, 10);
    }

    // ── By list breakdown ────────────────────────────────────────────────

    private VisualNode RenderListBreakdown(DashboardStats s) =>
        Border(
            VStack(
                Label("BY LIST")
                    .FontSize(11)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextSecondary)
                    .CharacterSpacing(1.5)
                    .Margin(0, 0, 0, 14),

                VStack(
                    s.ArticlesPerList.Select(ls =>
                    {
                        double ratio = s.TotalArticles > 0
                            ? (double)ls.Count / s.TotalArticles : 0;
                        var color = Color.FromArgb(ls.ListColor);

                        return (VisualNode)VStack(
                            HStack(
                                BoxView()
                                    .Color(color)
                                    .WidthRequest(10).HeightRequest(10)
                                    .CornerRadius(5)
                                    .VCenter(),
                                Label(ls.ListName)
                                    .FontSize(14)
                                    .TextColor(AppColors.TextPrimary)
                                    .HFill(),
                                Border(
                                    Label(ls.Count.ToString())
                                        .FontSize(12)
                                        .FontAttributes(FontAttributes.Bold)
                                        .TextColor(color)
                                        .Padding(8, 3)
                                )
                                .StrokeShape(new RoundRectangle().CornerRadius(8))
                                .BackgroundColor(color.WithAlpha(0.12f))
                                .Stroke(Colors.Transparent)
                            )
                            .Spacing(10),
                            ProgressBar()
                                .Progress(ratio)
                                .ProgressColor(color.WithAlpha(0.7f))
                                .BackgroundColor(AppColors.Surface)
                                .HeightRequest(5)
                                .Margin(20, 4, 0, 0)
                        )
                        .Spacing(0);
                    }).ToArray()
                )
                .Spacing(14)
            )
            .Padding(18, 16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(16))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 10);

    // ── Tag cloud ────────────────────────────────────────────────────────

    private VisualNode RenderTagCloud(DashboardStats s) =>
        Border(
            VStack(
                Label("TOP TAGS")
                    .FontSize(11)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextSecondary)
                    .CharacterSpacing(1.5)
                    .Margin(0, 0, 0, 12),

                FlexLayout(
                    s.TopTags.Select(t => (VisualNode)
                        Border(
                            HStack(
                                Label(t.TagName)
                                    .FontSize(12)
                                    .TextColor(AppColors.TextPrimary),
                                Border(
                                    Label(t.Count.ToString())
                                        .FontSize(10)
                                        .FontAttributes(FontAttributes.Bold)
                                        .TextColor(Color.FromArgb(t.TagColor))
                                        .Padding(5, 2)
                                )
                                .StrokeShape(new RoundRectangle().CornerRadius(8))
                                .BackgroundColor(Color.FromArgb(t.TagColor).WithAlpha(0.15f))
                                .Stroke(Colors.Transparent)
                            )
                            .Spacing(6)
                            .Padding(12, 7)
                        )
                        .StrokeShape(new RoundRectangle().CornerRadius(24))
                        .BackgroundColor(AppColors.TagBg)
                        .Stroke(Color.FromArgb(t.TagColor).WithAlpha(0.4f))
                        .StrokeThickness(1)
                        .Margin(0, 4, 6, 4)
                    ).ToArray()
                )
                .Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
                .Direction(Microsoft.Maui.Layouts.FlexDirection.Row)
            )
            .Padding(18, 16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(16))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 10);

    // ── Library stats (lists + tags counts) ─────────────────────────────

    private VisualNode RenderLibraryStats(DashboardStats s) =>
        VStack(
            Label("LIBRARY")
                .FontSize(11)
                .FontAttributes(FontAttributes.Bold)
                .TextColor(AppColors.TextSecondary)
                .CharacterSpacing(1.5)
                .Margin(20, 20, 20, 12),

            Grid(rows: "Auto", columns: "*, *",
                LibraryCard("🗂️", "Lists", s.TotalLists, "#C77DFF")
                    .Margin(20, 0, 8, 0),
                LibraryCard("🏷️", "Tags",  s.TotalTags,  "#FF9A3C")
                    .Margin(8, 0, 20, 0)
                    .GridColumn(1)
            )
        )
        .Spacing(0);

    private static MauiReactor.Border LibraryCard(string icon, string label, int value, string hex) =>
        Border(
            HStack(
                Label(icon).FontSize(28).VCenter(),
                VStack(
                    Label(value.ToString())
                        .FontSize(22)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(Color.FromArgb(hex)),
                    Label(label)
                        .FontSize(12)
                        .TextColor(AppColors.TextMuted)
                )
                .Spacing(1)
            )
            .Spacing(14)
            .Padding(16, 14)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(16))
        .BackgroundColor(AppColors.Card)
        .Stroke(Color.FromArgb(hex).WithAlpha(0.18f))
        .StrokeThickness(1);
}
