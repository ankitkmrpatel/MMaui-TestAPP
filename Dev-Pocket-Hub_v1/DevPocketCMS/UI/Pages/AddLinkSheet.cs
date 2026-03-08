using DevPocketCMS.Application.DTOs;
using DevPocketCMS.Application.Services;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class AddLinkState
{
    public string Url { get; set; } = string.Empty;
    public string ManualTitle { get; set; } = string.Empty;
    public int SelectedListId { get; set; }
    public List<int> SelectedTagIds { get; set; } = new();
    public bool FetchPreview { get; set; } = true;
    public bool EnablePocket { get; set; }
    public bool IsSaving { get; set; }
    public string? Error { get; set; }
    public List<TagDto> AllTags { get; set; } = new();
}

partial class AddLinkSheet : Component<AddLinkState>
{
    private List<ListDto> _lists = new();
    private Action<ArticleDto>? _onSaved;
    private Action? _onDismiss;

    [Inject] ArticleService _articleService;
    [Inject] TagService _tagService;

    public AddLinkSheet Lists(List<ListDto> lists) { _lists = lists; return this; }
    public AddLinkSheet OnSaved(Action<ArticleDto> cb) { _onSaved = cb; return this; }
    public AddLinkSheet OnDismiss(Action cb) { _onDismiss = cb; return this; }

    protected override async void OnMounted()
    {
        var tags = await _tagService.GetAllAsync();
        int defaultList = _lists.FirstOrDefault()?.Id ?? 0;
        SetState(s => { s.AllTags = tags; s.SelectedListId = defaultList; });
    }

    public override VisualNode Render() =>
        // Simulated bottom sheet using absolute layout overlay
        Grid(
            // Dim background
            BoxView()
                .Color(Color.FromArgb("#99000000"))
                .OnTapped(_onDismiss),

            // Sheet panel
            Border(
                ScrollView(
                    VStack(
                        // Handle bar
                        BoxView()
                            .Color(AppColors.CardBorder)
                            .WidthRequest(40).HeightRequest(4)
                            .CornerRadius(2)
                            .HCenter()
                            .Margin(0, 8, 0, 16),

                        Label("Save Link")
                            .FontSize(20)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(AppColors.TextPrimary),

                        // URL field
                        FieldLabel("URL *"),
                        TextInput(State.Url, "https://…", t => SetState(s => s.Url = t)),

                        // Manual title
                        FieldLabel("Title (optional – auto-fetched)"),
                        TextInput(State.ManualTitle, "Article title…",
                            t => SetState(s => s.ManualTitle = t)),

                        // List picker
                        FieldLabel("Save to List"),
                        Picker()
                            .Title("Select list")
                            .ItemsSource(_lists.Select(l => l.Name).ToList())
                            .SelectedIndex(Math.Max(0, _lists.FindIndex(l => l.Id == State.SelectedListId)))
                            .TextColor(AppColors.TextPrimary)
                            .BackgroundColor(AppColors.Card)
                            .TitleColor(AppColors.TextMuted)
                            .OnSelectedIndexChanged(idx =>
                            {
                                if (idx >= 0 && idx < _lists.Count)
                                    SetState(s => s.SelectedListId = _lists[idx].Id);
                            }),

                        // Tags
                        FieldLabel("Tags"),
                        ScrollView(
                            HStack(
                                State.AllTags.Select(t => (VisualNode)
                                    Border(
                                        Label(t.Name)
                                            .FontSize(12)
                                            .TextColor(State.SelectedTagIds.Contains(t.Id)
                                                ? AppColors.Background
                                                : AppColors.TextPrimary)
                                            .Padding(10, 6)
                                    )
                                    .StrokeShape(new RoundRectangle().CornerRadius(20))
                                    .BackgroundColor(State.SelectedTagIds.Contains(t.Id)
                                        ? Color.FromArgb(t.Color) : AppColors.Card)
                                    .Stroke(Color.FromArgb(t.Color))
                                    .StrokeThickness(1)
                                    .OnTapped(() => ToggleTag(t.Id))
                                ).ToArray()
                            )
                            .Spacing(6)
                        )
                        .Orientation(ScrollOrientation.Horizontal),

                        // Options
                        HStack(
                            CheckBox()
                                .IsChecked(State.FetchPreview)
                                .Color(AppColors.Accent)
                                .OnCheckedChanged(v => SetState(s => s.FetchPreview = v)),
                            Label("Auto-fetch preview (title, image, description)")
                                .FontSize(13)
                                .TextColor(AppColors.TextSecondary)
                                .VCenter()
                        )
                        .Spacing(8),

                        HStack(
                            CheckBox()
                                .IsChecked(State.EnablePocket)
                                .Color(AppColors.Accent)
                                .OnCheckedChanged(v => SetState(s => s.EnablePocket = v)),
                            Label("Pocket mode – save full HTML offline")
                                .FontSize(13)
                                .TextColor(AppColors.TextSecondary)
                                .VCenter()
                        )
                        .Spacing(8),

                        // Error message
                        State.Error is not null
                            ? Label(State.Error)
                                .FontSize(13)
                                .TextColor(AppColors.Danger)
                            : null,

                        // Save button
                        Button(State.IsSaving ? "Saving…" : "Save Article")
                            .BackgroundColor(AppColors.Accent)
                            .TextColor(AppColors.Background)
                            .FontAttributes(FontAttributes.Bold)
                            .CornerRadius(14)
                            .HeightRequest(54)
                            .IsEnabled(!State.IsSaving && !string.IsNullOrWhiteSpace(State.Url))
                            .OnClicked(SaveArticle),

                        Button("Cancel")
                            .BackgroundColor(Colors.Transparent)
                            .TextColor(AppColors.TextSecondary)
                            .FontSize(14)
                            .OnClicked(_onDismiss)
                    )
                    .Spacing(10)
                    .Padding(20, 0, 20, 40)
                )
            )
            .StrokeShape(new RoundRectangle().CornerRadius(new CornerRadius(20, 20, 0, 0)))
            .BackgroundColor(AppColors.Surface)
            .Stroke(AppColors.CardBorder)
            .StrokeThickness(1)
            .VEnd()
            .HFill()
        );

    private static VisualNode FieldLabel(string text) =>
        Label(text)
            .FontSize(12)
            .TextColor(AppColors.TextSecondary)
            .Margin(0, 4, 0, 2);

    private static VisualNode TextInput(string value, string placeholder, Action<string> onChange) =>
        Border(
            Entry()
                .Text(value)
                .Placeholder(placeholder)
                .PlaceholderColor(AppColors.TextMuted)
                .TextColor(AppColors.TextPrimary)
                .BackgroundColor(Colors.Transparent)
                .OnTextChanged(onChange)
                .Margin(12, 0)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(10))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .HeightRequest(48);

    private void ToggleTag(int id) =>
        SetState(s =>
        {
            if (s.SelectedTagIds.Contains(id)) s.SelectedTagIds.Remove(id);
            else s.SelectedTagIds.Add(id);
        });

    private async void SaveArticle()
    {
        var url = State.Url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            SetState(s => s.Error = "Please enter a valid URL (include https://).");
            return;
        }

        SetState(s => { s.IsSaving = true; s.Error = null; });

        try
        {
            var article = await _articleService.SaveAsync(
                url: url,
                listId: State.SelectedListId,
                tagIds: State.SelectedTagIds,
                manualTitle: string.IsNullOrWhiteSpace(State.ManualTitle) ? null : State.ManualTitle,
                fetchPreview: State.FetchPreview,
                enablePocketMode: State.EnablePocket);

            _onSaved?.Invoke(article);
        }
        catch (Exception ex)
        {
            SetState(s => { s.IsSaving = false; s.Error = $"Error: {ex.Message}"; });
        }
    }
}
