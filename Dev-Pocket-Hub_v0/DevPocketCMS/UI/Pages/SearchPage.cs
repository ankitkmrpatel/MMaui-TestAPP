using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Application.Services;
using DevPocketCMS.Core.Models;
using DevPocketCMS.UI.Components;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class SearchState
{
    public string Query { get; set; } = string.Empty;
    public SearchLogic Logic { get; set; } = SearchLogic.OR;
    public List<TagDto> AllTags { get; set; } = new();
    public List<ListDto> AllLists { get; set; } = new();
    public HashSet<int> TagFilter { get; set; } = new();
    public HashSet<int> ListFilter { get; set; } = new();
    public bool? ReadFilter { get; set; }
    public bool? FavFilter { get; set; }
    public List<ArticleDto> Results { get; set; } = new();
    public bool HasSearched { get; set; }
    public bool IsLoading { get; set; }
    public bool ShowFilters { get; set; }
}

partial class SearchPage : Component<SearchState>
{
    [Inject] SearchService _search;
    [Inject] TagService _tagService;
    [Inject] ListService _listService;

    protected override async void OnMounted()
    {
        var tags = await _tagService.GetAllAsync();
        var lists = await _listService.GetAllAsync();
        SetState(s => { s.AllTags = tags; s.AllLists = lists; });
    }

    public override VisualNode Render() =>
        ContentPage(
            VStack(
                // ── Search bar + filter toggle ─────────────────────────────
                Grid(rows: "*", columns: "*, Auto",
                    Border(
                        HStack(
                            Label("🔍").FontSize(16).VCenter().Margin(12, 0, 4, 0),
                            Entry()
                                .Placeholder("Search articles, tags, notes…")
                                .PlaceholderColor(AppColors.TextMuted)
                                .TextColor(AppColors.TextPrimary)
                                .BackgroundColor(Colors.Transparent)
                                .Text(State.Query)
                                .ReturnType(ReturnType.Search)
                                .OnTextChanged(t => SetState(s => s.Query = t))
                                .OnCompleted(ExecuteSearch)
                                .HFill()
                        )
                        .Spacing(0)
                    )
                    .StrokeShape(new RoundRectangle().CornerRadius(14))
                    .BackgroundColor(AppColors.Card)
                    .Stroke(AppColors.CardBorder)
                    .StrokeThickness(1)
                    .HeightRequest(50)
                    .HFill(),

                    Button(State.ShowFilters ? "✕ Filters" : "Filters")
                        .BackgroundColor(State.ShowFilters ? AppColors.AccentBg : AppColors.Card)
                        .TextColor(State.ShowFilters ? AppColors.Accent : AppColors.TextSecondary)
                        .CornerRadius(10)
                        .FontSize(13)
                        .Margin(8, 0, 0, 0)
                        .GridColumn(1)
                        .OnClicked(() => SetState(s => s.ShowFilters = !s.ShowFilters))
                )
                .Padding(20, 16, 20, 8),

                // ── Filters panel ──────────────────────────────────────────
                State.ShowFilters ? RenderFilters() : null,

                // ── Results ────────────────────────────────────────────────
                State.IsLoading
                    ? ActivityIndicator()
                        .IsRunning(true)
                        .Color(AppColors.Accent)
                        .HCenter()
                        .Margin(0, 32)
                    : !State.HasSearched
                        ? Label("Type to search or apply filters below.")
                            .FontSize(15)
                            .TextColor(AppColors.TextMuted)
                            .HCenter()
                            .Margin(0, 48)
                        : State.Results.Count == 0
                            ? new EmptyState()
                                .Icon("🔍")
                                .Message("No results found.\nTry different keywords or filters.")
                            : CollectionView()
                                .ItemsSource(State.Results, a => new ArticleCard().Article(a))
#warning 1,1,1 Check this Issue
            //.ContentInsetAdjustmentBehavior(ScrollViewContentInsetAdjustmentBehavior.Always)
            )
            .Spacing(0)
        )
        .BackgroundColor(AppColors.Background)
        .Title(string.Empty);

    private VisualNode RenderFilters() =>
        Border(
            VStack(
                // Logic toggle
                HStack(
                    Label("Logic:").FontSize(13).TextColor(AppColors.TextSecondary).VCenter(),
                    Button("OR")
                        .BackgroundColor(State.Logic == SearchLogic.OR ? AppColors.Accent : AppColors.Surface)
                        .TextColor(State.Logic == SearchLogic.OR ? AppColors.Background : AppColors.TextSecondary)
                        .CornerRadius(8).HeightRequest(32).WidthRequest(54)
                        .OnClicked(() => SetState(s => s.Logic = SearchLogic.OR)),
                    Button("AND")
                        .BackgroundColor(State.Logic == SearchLogic.AND ? AppColors.Accent : AppColors.Surface)
                        .TextColor(State.Logic == SearchLogic.AND ? AppColors.Background : AppColors.TextSecondary)
                        .CornerRadius(8).HeightRequest(32).WidthRequest(54)
                        .OnClicked(() => SetState(s => s.Logic = SearchLogic.AND))
                )
                .Spacing(8),

                // Tag filters
                Label("Tags").FontSize(13).TextColor(AppColors.TextSecondary),
                ScrollView(
                    HStack(
                        State.AllTags.Select(t => (VisualNode)
                            Border(Label(t.Name).FontSize(12)
                                .TextColor(State.TagFilter.Contains(t.Id)
                                    ? AppColors.Background : AppColors.TextPrimary)
                                .Padding(10, 6)
                            )
                            .StrokeShape(new RoundRectangle().CornerRadius(20))
                            .BackgroundColor(State.TagFilter.Contains(t.Id)
                                ? Color.FromArgb(t.Color) : AppColors.Card)
                            .Stroke(Color.FromArgb(t.Color))
                            .StrokeThickness(1)
                            .OnTapped(() => ToggleTag(t.Id))
                        ).ToArray()
                    )
                    .Spacing(6)
                )
                .Orientation(ScrollOrientation.Horizontal),

                // Read / Fav toggles
                HStack(
                    FilterToggle("Unread", State.ReadFilter == false,
                        () => SetState(s => s.ReadFilter = s.ReadFilter == false ? null : false)),
                    FilterToggle("Favorites", State.FavFilter == true,
                        () => SetState(s => s.FavFilter = s.FavFilter == true ? null : true))
                )
                .Spacing(8),

                Button("Search")
                    .BackgroundColor(AppColors.Accent)
                    .TextColor(AppColors.Background)
                    .FontAttributes(FontAttributes.Bold)
                    .CornerRadius(10).HeightRequest(44)
                    .OnClicked(ExecuteSearch)
            )
            .Spacing(10)
            .Padding(16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 0, 20, 8);

    private static VisualNode FilterToggle(string label, bool isActive, Action onTap) =>
        Border(
            Label(label).FontSize(13)
                .TextColor(isActive ? AppColors.Background : AppColors.TextSecondary)
                .Padding(12, 6)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(20))
        .BackgroundColor(isActive ? AppColors.Accent : AppColors.Surface)
        .Stroke(isActive ? AppColors.Accent : AppColors.CardBorder)
        .StrokeThickness(1)
        .OnTapped(onTap);

    private void ToggleTag(int id)
    {
        SetState(s =>
        {
            if (s.TagFilter.Contains(id)) s.TagFilter.Remove(id);
            else s.TagFilter.Add(id);
        });
    }

    private async void ExecuteSearch()
    {
        SetState(s => { s.IsLoading = true; s.HasSearched = true; });

        var opts = new SearchOptions
        {
            Query = State.Query,
            Logic = State.Logic,
            TagIds = State.TagFilter.ToList(),
            ListIds = State.ListFilter.ToList(),
            IsRead = State.ReadFilter,
            IsFavorite = State.FavFilter,
        };

        var results = await _search.SearchAsync(opts);
        SetState(s => { s.Results = results; s.IsLoading = false; });
    }
}
