using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Application.Services;
using DevPocketCMS.UI.Components;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class HomeState
{
    public List<ArticleDto> Articles { get; set; } = new();
    public List<ListDto> Lists { get; set; } = new();
    public int? SelectedListId { get; set; }
    public bool IsLoading { get; set; } = true;
    public bool ShowAddSheet { get; set; }
    public string FilterLabel { get; set; } = "All";
}

partial class HomePage : Component<HomeState>
{
    [Inject] readonly ArticleService _articleService;
    [Inject] readonly ListService _listService;

    protected override async void OnMounted() => await LoadAsync();

    private async Task LoadAsync()
    {
        SetState(s => s.IsLoading = true);

        var articles = State.SelectedListId.HasValue
            ? await _articleService.GetByListAsync(State.SelectedListId.Value)
            : await _articleService.GetAllAsync();

        var lists = await _listService.GetAllAsync();

        SetState(s =>
        {
            s.Articles = articles;
            s.Lists = lists;
            s.IsLoading = false;
        });
    }

    public override VisualNode Render() =>
        ContentPage(
            Grid(rows: "Auto, Auto, *", columns: "*",

                // ── Header ─────────────────────────────────────────────────
                RenderHeader().GridRow(0),

                // ── List filter chips ────────────────────────────────────
                RenderListFilter().GridRow(1),

                // ── Article feed ─────────────────────────────────────────
                State.IsLoading
                    ? ActivityIndicator()
                        .IsRunning(true)
                        .Color(AppColors.Accent)
                        .HCenter()
                        .VCenter()
                        .GridRow(2)
                    : State.Articles.Count == 0
                        ? new EmptyState()
                            .Icon("📄")
                            .Message("No articles yet.\nTap + to save your first link.")
                            .GridRow(2)
                        : CollectionView()
                            .ItemsSource(State.Articles, article =>
                                new ArticleCard()
                                    .Article(article)
                                    .OnToggleRead(ToggleRead)
                                    .OnToggleFavorite(ToggleFavorite)
                                    .OnDelete(DeleteArticle)
                            )
#warning 1,1,1 Check this Issue
                            //.ContentInsetAdjustmentBehavior(ScrollViewContentInsetAdjustmentBehavior.Always)
                            .GridRow(2),

                // ── FAB ──────────────────────────────────────────────────
                Border(
                    Label("+")
                        .FontSize(28)
                        .TextColor(AppColors.Background)
                        .FontAttributes(FontAttributes.Bold)
                        .HCenter()
                        .VCenter()
                )
                .StrokeShape(new RoundRectangle().CornerRadius(32))
                .BackgroundColor(AppColors.Accent)
                .WidthRequest(60)
                .HeightRequest(60)
                .HEnd()
                .VEnd()
                .Margin(0, 0, 20, 100)
                .GridRow(2)
                .ZIndex(10)
                .OnTapped(() => SetState(s => s.ShowAddSheet = true))
            ),

            // ── Add Link Bottom Sheet ─────────────────────────────────────
            State.ShowAddSheet
                ? new AddLinkSheet()
                    .Lists(State.Lists)
                    .OnSaved((a) => { SetState(s => s.ShowAddSheet = false); _ = LoadAsync(); })
                    .OnDismiss(() => SetState(s => s.ShowAddSheet = false))
                : null
        )
        .BackgroundColor(AppColors.Background)
        .Title(string.Empty);

    private VisualNode RenderHeader() =>
        Grid(rows: "*", columns: "*, Auto",
            VStack(
                Label("DevPocketCMS")
                    .FontSize(11)
                    .TextColor(AppColors.Accent)
                    .FontAttributes(FontAttributes.Bold)
                    .CharacterSpacing(1.5),
                Label(State.FilterLabel)
                    .FontSize(26)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextPrimary)
            )
            .Spacing(2),

            Label($"{State.Articles.Count(a => !a.IsRead)} unread")
                .FontSize(13)
                .TextColor(AppColors.TextMuted)
                .VCenter()
                .GridColumn(1)
        )
        .Padding(20, 16, 20, 8);

    private VisualNode RenderListFilter() =>
        ScrollView(
            HStack(
                // "All" chip
                [new FilterChip()
                    .WithLabel("All")
                    .SetIsSelected(!State.SelectedListId.HasValue)
                    .WithColor(AppColors.Accent)
                    .SetOnTap(() => ApplyFilter(null, "All")),
                
                // Per-list chips
                ..State.Lists.Select(l => (VisualNode)
                    new FilterChip()
                        .WithLabel(l.Name)
                        .SetIsSelected(State.SelectedListId == l.Id)
                        .WithColor(Color.FromArgb(l.Color))
                        .SetOnTap(() => ApplyFilter(l.Id, l.Name))
                )]
            )
            .Spacing(8)
            .Padding(20, 0)
        )
        .Orientation(ScrollOrientation.Horizontal)
        .Padding(0, 4, 0, 8);

    private void ApplyFilter(int? listId, string label)
    {
        SetState(s =>
        {
            s.SelectedListId = listId;
            s.FilterLabel = label;
        });
        _ = LoadAsync();
    }

    private async void ToggleRead(int id)
    {
        var article = State.Articles.FirstOrDefault(a => a.Id == id);
        if (article is null) return;
        await _articleService.MarkReadAsync(id, !article.IsRead);
        await LoadAsync();
    }

    private async void ToggleFavorite(int id)
    {
        await _articleService.ToggleFavoriteAsync(id);
        await LoadAsync();
    }

    private async void DeleteArticle(int id)
    {
        await _articleService.DeleteAsync(id);
        await LoadAsync();
    }
}
