using DevPocketCMS.Application.Services;
using DevPocketCMS.Core.Models;
using DevPocketCMS.UI.Components;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class DashboardState
{
    public DashboardStats? Stats { get; set; }
    public bool IsLoading { get; set; } = true;
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

    public override VisualNode Render() =>
        ContentPage(
            ScrollView(
                VStack(
                    // ── Title ──────────────────────────────────────────────
                    Label("Dashboard")
                        .FontSize(26)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.TextPrimary)
                        .Margin(20, 16, 20, 4),

                    State.IsLoading
                        ? ActivityIndicator()
                            .IsRunning(true)
                            .Color(AppColors.Accent)
                            .HCenter()
                            .Margin(0, 32)
                        : State.Stats is null
                            ? null
                            : RenderStats(State.Stats)
                )
                .Spacing(0)
            )
#warning 1,1,1 Check this Issue
        //.ContentInsetAdjustmentBehavior(ScrollViewContentInsetAdjustmentBehavior.Always)
        )
        .BackgroundColor(AppColors.Background)
        .Title(string.Empty);

    private VisualNode RenderStats(DashboardStats s) =>
        VStack(
            // ── Main KPI grid ─────────────────────────────────────────────
            Label("Overview")
                .FontSize(13)
                .FontAttributes(FontAttributes.Bold)
                .TextColor(AppColors.TextSecondary)
                .CharacterSpacing(1)
                .Margin(20, 12, 20, 8),

            Grid(rows: "Auto, Auto", columns: "*, *",
                new StatCard().SetLabel("Total").SetValue(s.TotalArticles).HasIcon("📚").WithColor(AppColors.Accent).ViewMargin(10),
                new StatCard().SetLabel("Unread").SetValue(s.UnreadArticles).HasIcon("🔵").WithColor(Color.FromArgb("#00C2FF")).ViewMargin(10).GridColumn(1),
                new StatCard().SetLabel("Read").SetValue(s.ReadArticles).HasIcon("✅").WithColor(Color.FromArgb("#4FFFB0")).ViewMargin(10).GridRow(1),
                new StatCard().SetLabel("Favorites").SetValue(s.FavoriteArticles).HasIcon("⭐").WithColor(Color.FromArgb("#FFD166")).ViewMargin(10).GridRow(1).GridColumn(1)
            )
            .Padding(10, 0),

            // ── Read progress bar ────────────────────────────────────────
            s.TotalArticles > 0
                ? RenderProgressSection(s)
                : null,

            // ── Articles per list ─────────────────────────────────────────
            s.ArticlesPerList.Any()
                ? RenderListBreakdown(s)
                : null,

            // ── Top tags ──────────────────────────────────────────────────
            s.TopTags.Any()
                ? RenderTagCloud(s)
                : null,

            // ── Secondary stats ───────────────────────────────────────────
            Grid(rows: "Auto, Auto", columns: "*, *",
                new StatCard().SetLabel("Lists").SetValue(s.TotalLists).HasIcon("🗂️").WithColor(Color.FromArgb("#C77DFF")).ViewMargin(10),
                new StatCard().SetLabel("Tags").SetValue(s.TotalTags).HasIcon("🏷️").WithColor(Color.FromArgb("#FF9A3C")).ViewMargin(10).GridColumn(1),
                new StatCard().SetLabel("Pocket Saved").SetValue(s.PocketSavedArticles).HasIcon("💾").WithColor(Color.FromArgb("#44D7B6")).ViewMargin(10).GridRow(1)
            )
            .Padding(10, 0)
            .Margin(0, 0, 0, 20)
        )
        .Spacing(0);

    private VisualNode RenderProgressSection(DashboardStats s)
    {
        double progress = s.TotalArticles > 0 ? (double)s.ReadArticles / s.TotalArticles : 0;
        return Border(
            VStack(
                HStack(
                    Label("Reading Progress").FontSize(15).FontAttributes(FontAttributes.Bold).TextColor(AppColors.TextPrimary).HFill(),
                    Label($"{(int)(progress * 100)}%").FontSize(15).FontAttributes(FontAttributes.Bold).TextColor(AppColors.Accent)
                ),
                ProgressBar()
                    .Progress(progress)
                    .ProgressColor(AppColors.Accent)
                    .BackgroundColor(AppColors.Surface)
                    .HeightRequest(8)
            )
            .Spacing(10)
            .Padding(16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 8);
    }

    private VisualNode RenderListBreakdown(DashboardStats s) =>
        Border(
            VStack(
                Label("By List")
                    .FontSize(15).FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextPrimary),

                VStack(
                    s.ArticlesPerList.Select(ls =>
                    {
                        double ratio = s.TotalArticles > 0 ? (double)ls.Count / s.TotalArticles : 0;
                        return (VisualNode)VStack(
                            HStack(
                                BoxView().Color(Color.FromArgb(ls.ListColor)).WidthRequest(10).HeightRequest(10).CornerRadius(5).VCenter(),
                                Label(ls.ListName).FontSize(13).TextColor(AppColors.TextSecondary).HFill(),
                                Label(ls.Count.ToString()).FontSize(13).FontAttributes(FontAttributes.Bold).TextColor(AppColors.TextPrimary)
                            )
                            .Spacing(8),
                            ProgressBar()
                                .Progress(ratio)
                                .ProgressColor(Color.FromArgb(ls.ListColor))
                                .BackgroundColor(AppColors.Surface)
                                .HeightRequest(5)
                        )
                        .Spacing(4);
                    }).ToArray()
                )
                .Spacing(12)
            )
            .Spacing(12)
            .Padding(16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 8);

    private VisualNode RenderTagCloud(DashboardStats s) =>
        Border(
            VStack(
                Label("Top Tags")
                    .FontSize(15).FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextPrimary),

                FlexLayout(
                    s.TopTags.Select(t => (VisualNode)
                        Border(
                            HStack(
                                Label(t.TagName).FontSize(12).TextColor(AppColors.TextPrimary),
                                Label(t.Count.ToString())
                                    .FontSize(11)
                                    .TextColor(Color.FromArgb(t.TagColor))
                                    .FontAttributes(FontAttributes.Bold)
                            )
                            .Spacing(6)
                            .Padding(10, 6)
                        )
                        .StrokeShape(new RoundRectangle().CornerRadius(20))
                        .BackgroundColor(AppColors.TagBg)
                        .Stroke(Color.FromArgb(t.TagColor))
                        .StrokeThickness(1)
                        .Margin(0, 4, 6, 4)
                    ).ToArray()
                )
                .Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
                .Direction(Microsoft.Maui.Layouts.FlexDirection.Row)
            )
            .Spacing(12)
            .Padding(16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 8);
}
