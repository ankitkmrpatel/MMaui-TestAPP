using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class OnboardingState
{
    public int CurrentStep { get; set; }
    public string UserType { get; set; } = "Developer";
    public string Theme { get; set; } = "system";
    public bool IsSaving { get; set; }
}

partial class OnboardingPage : Component<OnboardingState>
{
    private Action? _onComplete;

    [Inject] readonly IPreferencesRepository _prefs;

    public OnboardingPage OnComplete(Action action)
    {
        _onComplete = action;
        return this;
    }

    private static readonly (string Icon, string Title, string Body)[] Steps =
    {
        ("📚", "Welcome to\nDevPocketCMS",
            "Your personal CMS for saving developer articles, blog posts, and links — all in one place."),
        ("🗂️", "Organise with Lists & Tags",
            "Create custom reading lists and tag content for lightning-fast retrieval."),
        ("🔍", "Advanced Search",
            "Search by keyword, tag, or list. Combine filters with AND / OR logic."),
        ("🔖", "Pocket Mode",
            "Save full page content offline. Read even without internet."),
        ("⚙️", "Your Preferences",
            "Choose your theme and get started."),
    };

    public override VisualNode Render()
    {
        var (icon, title, body) = Steps[State.CurrentStep];
        bool isLast = State.CurrentStep == Steps.Length - 1;

        return ContentPage(
            Grid(rows: "*, Auto", columns: "*",
                // ── Slide content ──────────────────────────────────────────
                ScrollView(
                    VStack(
                        Label(icon)
                            .FontSize(72)
                            .HCenter()
                            .Margin(0, 60, 0, 24),

                        Label(title)
                            .FontSize(28)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(AppColors.TextPrimary)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .Margin(24, 0),

                        Label(body)
                            .FontSize(16)
                            .TextColor(AppColors.TextSecondary)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .LineHeight(1.5)
                            .Margin(32, 16),

                        // ── Preference step ───────────────────────────────
                        isLast ? RenderPreferences() : null
                    )
                    .Spacing(0)
                )
                .GridRow(0),

                // ── Navigation controls ────────────────────────────────────
                VStack(
                    // Dot indicators
                    HStack(
                        [.. Steps.Select((_, i) => (VisualNode)
                            BoxView()
                                .WidthRequest(i == State.CurrentStep ? 24 : 8)
                                .HeightRequest(8)
                                .CornerRadius(4)
                                .Color(i == State.CurrentStep
                                    ? AppColors.Accent
                                    : AppColors.TextMuted)
                                .Margin(3, 0)
                        )]
                    )
                    .HCenter()
                    .Margin(0, 0, 0, 24),

                    // Action button
                    Button(isLast ? (State.IsSaving ? "Setting up…" : "Get Started") : "Continue")
                        .BackgroundColor(AppColors.Accent)
                        .TextColor(AppColors.Background)
                        .FontAttributes(FontAttributes.Bold)
                        .FontSize(16)
                        .CornerRadius(14)
                        .HeightRequest(54)
                        .IsEnabled(!State.IsSaving)
                        .OnClicked(isLast ? SaveAndContinue : NextStep)
                        .Margin(24, 0),

                    // Back link (not on first step)
                    State.CurrentStep > 0
                        ? Button("Back")
                            .BackgroundColor(Colors.Transparent)
                            .TextColor(AppColors.TextSecondary)
                            .FontSize(14)
                            .OnClicked(() => SetState(s => s.CurrentStep--))
                            .Margin(24, 4, 24, 0)
                        : null
                )
                .GridRow(1)
                .Margin(0, 0, 0, 32)
            )
        )
        .BackgroundColor(AppColors.Background);
    }

    private VisualNode RenderPreferences() =>
        VStack(
            // Theme picker
            Label("Theme")
                .FontSize(14)
                .TextColor(AppColors.TextSecondary)
                .Margin(32, 24, 32, 8),

            HStack(
                new[] { ("Light", "light"), ("Dark", "dark"), ("System", "system") }
                    .Select(t => (VisualNode)
                        Border(
                            Label(t.Item1)
                                .FontSize(14)
                                .TextColor(State.Theme == t.Item2
                                    ? AppColors.Background
                                    : AppColors.TextPrimary)
                                .HCenter()
                                .VCenter()
                        )
                        .StrokeShape(new RoundRectangle().CornerRadius(10))
                        .BackgroundColor(State.Theme == t.Item2 ? AppColors.Accent : AppColors.Card)
                        .Stroke(State.Theme == t.Item2 ? AppColors.Accent : AppColors.CardBorder)
                        .StrokeThickness(1)
                        .HeightRequest(44)
                        .WidthRequest(90)
                        .OnTapped(() => SetState(s => s.Theme = t.Item2))
                    ).ToArray()
            )
            .HCenter()
            .Spacing(10)
        )
        .Spacing(0);

    private void NextStep() => SetState(s => s.CurrentStep++);

    private async void SaveAndContinue()
    {
        SetState(s => s.IsSaving = true);

        var prefs = await _prefs.GetAsync();
        prefs.UserType = "Developer";
        prefs.Theme = State.Theme;
        prefs.HasCompletedOnboarding = true;

        await _prefs.SaveAsync(prefs);

        SetState(s => s.IsSaving = false);
        _onComplete?.Invoke();
    }
}
