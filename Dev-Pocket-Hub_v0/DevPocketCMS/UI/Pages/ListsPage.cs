using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Application.Services;
using DevPocketCMS.UI.Components;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class ListsState
{
    public List<ListDto> Lists { get; set; } = new();
    public bool IsLoading { get; set; } = true;
    public bool ShowCreate { get; set; }
    public string NewName { get; set; } = string.Empty;
    public string NewColor { get; set; } = "#4FFFB0";
    public string NewIcon { get; set; } = "bookmark";
}

partial class ListsPage : Component<ListsState>
{
    [Inject] readonly ListService _listService;

    protected override async void OnMounted() => await LoadAsync();

    private async Task LoadAsync()
    {
        SetState(s => s.IsLoading = true);
        var lists = await _listService.GetAllAsync();
        SetState(s => { s.Lists = lists; s.IsLoading = false; });
    }

    public override VisualNode Render() =>
        ContentPage(
            Grid(rows: "Auto, *", columns: "*",

                // Header
                Grid(rows: "*", columns: "*, Auto",
                    Label("Lists")
                        .FontSize(26)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.TextPrimary)
                        .VCenter(),

                    Button("New List")
                        .BackgroundColor(AppColors.AccentBg)
                        .TextColor(AppColors.Accent)
                        .CornerRadius(10)
                        .FontSize(14)
                        .GridColumn(1)
                        .OnClicked(() => SetState(s => s.ShowCreate = !s.ShowCreate))
                )
                .Padding(20, 16)
                .GridRow(0),

                State.IsLoading
                    ? ActivityIndicator()
                        .IsRunning(true)
                        .Color(AppColors.Accent)
                        .HCenter()
                        .VCenter()
                        .GridRow(1)
                    : ScrollView(
                        VStack(
                            // Create form
                            State.ShowCreate ? RenderCreateForm() : null,

                            State.Lists.Count == 0
                                ? new EmptyState()
                                    .Icon("📂")
                                    .Message("No lists yet. Create one!")
                                : null,

                            State.Lists.Select(list => (VisualNode)
                                RenderListCard(list)
                            ).ToArray()
                        )
                        .Spacing(12)
                        .Padding(20, 8)
                    )
                    .GridRow(1)
            )
        )
        .BackgroundColor(AppColors.Background)
        .Title(string.Empty);

    private VisualNode RenderCreateForm() =>
        Border(
            VStack(
                Label("New List")
                    .FontSize(16)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextPrimary),

                Entry()
                    .Placeholder("List name…")
                    .PlaceholderColor(AppColors.TextMuted)
                    .TextColor(AppColors.TextPrimary)
                    .BackgroundColor(AppColors.Surface)
                    .Text(State.NewName)
                    .OnTextChanged(t => SetState(s => s.NewName = t)),

                Label("Color")
                    .FontSize(13)
                    .TextColor(AppColors.TextSecondary),

                ScrollView(
                    HStack(
                        AppColors.ListColors.Select(hex => (VisualNode)
                            BoxView()
                                .Color(Color.FromArgb(hex))
                                .WidthRequest(State.NewColor == hex ? 36 : 28)
                                .HeightRequest(State.NewColor == hex ? 36 : 28)
                                .CornerRadius(18)
                                .Margin(4, 0)
                                .OnTapped(() => SetState(s => s.NewColor = hex))
                        ).ToArray()
                    )
                    .Spacing(2)
                )
                .Orientation(ScrollOrientation.Horizontal),

                HStack(
                    Button("Cancel")
                        .BackgroundColor(AppColors.Surface)
                        .TextColor(AppColors.TextSecondary)
                        .CornerRadius(10)
                        .HeightRequest(44)
                        .HFill()
                        .OnClicked(() => SetState(s => { s.ShowCreate = false; s.NewName = ""; })),

                    Button("Create")
                        .BackgroundColor(AppColors.Accent)
                        .TextColor(AppColors.Background)
                        .FontAttributes(FontAttributes.Bold)
                        .CornerRadius(10)
                        .HeightRequest(44)
                        .HFill()
                        .IsEnabled(!string.IsNullOrWhiteSpace(State.NewName))
                        .OnClicked(CreateList)
                )
                .Spacing(10)
            )
            .Spacing(12)
            .Padding(16)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1);

    private VisualNode RenderListCard(ListDto list) =>
        Border(
            Grid(rows: "*", columns: "Auto, *, Auto",
                BoxView()
                    .Color(Color.FromArgb(list.Color))
                    .WidthRequest(4)
                    .CornerRadius(2)
                    .VFill()
                    .Margin(0, 0, 14, 0),

                VStack(
                    Label(list.Name)
                        .FontSize(16)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.TextPrimary),
                    Label($"{list.ArticleCount} articles · {list.UnreadCount} unread")
                        .FontSize(12)
                        .TextColor(AppColors.TextMuted)
                )
                .Spacing(2)
                .GridColumn(1)
                .VCenter(),

                Button("Delete")
                    .BackgroundColor(Colors.Transparent)
                    .TextColor(AppColors.Danger)
                    .FontSize(13)
                    .GridColumn(2)
                    .VCenter()
                    .OnClicked(() => DeleteList(list.Id))
            )
            .Padding(16, 14)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1);

    private async void CreateList()
    {
        await _listService.CreateAsync(State.NewName.Trim(), State.NewColor, State.NewIcon);
        SetState(s => { s.ShowCreate = false; s.NewName = ""; s.NewColor = "#4FFFB0"; });
        await LoadAsync();
    }

    private async void DeleteList(int id)
    {
        bool confirmed = await Microsoft.Maui.Controls.Application.Current!.Windows[0].Page!
            .DisplayAlertAsync("Delete List", "Articles in this list will be moved to Reading List.",
            "Delete", "Cancel");

        if (!confirmed) return;

        await _listService.DeleteAsync(id);
        await LoadAsync();
    }
}
