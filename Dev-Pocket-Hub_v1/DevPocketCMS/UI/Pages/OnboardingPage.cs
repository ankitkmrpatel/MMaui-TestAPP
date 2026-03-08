using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class OnboardingState
{
    public int    CurrentStep   { get; set; }
    public string Theme         { get; set; } = "system";
    public bool   IsSaving      { get; set; }
    public double ContentOpacity { get; set; } = 1.0;
}

partial class OnboardingPage : Component<OnboardingState>
{
    private Action? _onComplete;

    [Inject] readonly IPreferencesRepository _prefs;

    public OnboardingPage OnComplete(Action action) { _onComplete = action; return this; }

    // ── Step data ─────────────────────────────────────────────────────────────

    private static readonly (string Icon, string Title, string Body, string AccentHex)[] Steps =
    {
        ("📚", "Welcome to\nDevPocketCMS",
            "Your personal CMS for developer articles, blog posts, and links — beautifully organised in one place.",
            "#4FFFB0"),
        ("🗂️", "Organise with\nLists & Tags",
            "Create reading lists, tag content by topic, and filter your feed in seconds.",
            "#00C2FF"),
        ("🔍", "Advanced Search",
            "Search by keyword, tag, or list. Combine conditions with AND / OR logic to find anything instantly.",
            "#C77DFF"),
        ("🔖", "Pocket Mode",
            "Save full page HTML offline. Read your favourite articles even without an internet connection.",
            "#44D7B6"),
        ("⚙️", "Choose Your Theme",
            "Pick a colour scheme that suits you. Everything is ready — let's get started!",
            "#FFD166"),
    };

    // ── Parallax background positions per step ──────────────────────────────
    // (bigCircleX, bigCircleY, smallCircleX, smallCircleY) — pixel offsets from anchors

    private static readonly (double BgX, double BgY, double SmX, double SmY)[] ParallaxOffsets =
    {
        ( 20,  -20,  -10,  10),
        (-30,   15,   20, -15),
        ( 10,  -30,  -25,  20),
        (-20,   25,   15, -30),
        ( 30,  -10,  -20,  25),
    };

    // ── Render ────────────────────────────────────────────────────────────────

    public override VisualNode Render()
    {
        var (icon, title, body, accentHex) = Steps[State.CurrentStep];
        var accent   = Color.FromArgb(accentHex);
        bool isLast  = State.CurrentStep == Steps.Length - 1;
        var (bgX, bgY, smX, smY) = ParallaxOffsets[State.CurrentStep];

        return ContentPage(
            Grid(rows: "*, Auto", columns: "*",

                // ────────────────── Background + illustration layer ──────────────
                Grid(
                    // Large background circle (parallax: moves ½ speed)
                    BoxView()
                        .Color(accent.WithAlpha(0.12f))
                        .WidthRequest(360)
                        .HeightRequest(360)
                        .CornerRadius(180)
                        .HEnd()
                        .VStart()
                        .TranslationX(bgX * 0.5)
                        .TranslationY(bgY * 0.5 - 40)
                        .WithAnimation(duration: 500, easing: Easing.CubicInOut),

                    // Medium accent ring (parallax: ¾ speed)
                    BoxView()
                        .Color(accent.WithAlpha(0.07f))
                        .WidthRequest(200)
                        .HeightRequest(200)
                        .CornerRadius(100)
                        .HStart()
                        .VEnd()
                        .TranslationX(smX * 0.75)
                        .TranslationY(smY * 0.75 + 30)
                        .WithAnimation(duration: 550, easing: Easing.CubicInOut),

                    // Tiny accent dot cluster
                    HStack(
                        BoxView().Color(accent.WithAlpha(0.4f)).WidthRequest(8).HeightRequest(8).CornerRadius(4),
                        BoxView().Color(accent.WithAlpha(0.25f)).WidthRequest(5).HeightRequest(5).CornerRadius(2.5f),
                        BoxView().Color(accent.WithAlpha(0.15f)).WidthRequest(4).HeightRequest(4).CornerRadius(2)
                    )
                    .Spacing(6)
                    .HCenter()
                    .VStart()
                    .Margin(0, 60, 0, 0)
                    .TranslationX(bgX * 0.3)
                    .WithAnimation(duration: 480, easing: Easing.CubicInOut),

                    // ── Slide content (full speed, fades on step change) ────────
                    ScrollView(
                        VStack(
                            // Accent bar above icon
                            BoxView()
                                .Color(accent)
                                .WidthRequest(40)
                                .HeightRequest(4)
                                .CornerRadius(2)
                                .HCenter()
                                .Margin(0, 70, 0, 0),

                            // Step badge
                            Border(
                                Label($"{State.CurrentStep + 1} / {Steps.Length}")
                                    .FontSize(11)
                                    .TextColor(accent)
                                    .FontAttributes(FontAttributes.Bold)
                                    .Padding(10, 4)
                            )
                            .StrokeShape(new RoundRectangle().CornerRadius(12))
                            .BackgroundColor(accent.WithAlpha(0.12f))
                            .Stroke(accent.WithAlpha(0.3f))
                            .StrokeThickness(1)
                            .HCenter()
                            .Margin(0, 12, 0, 0),

                            // Large icon
                            Label(icon)
                                .FontSize(80)
                                .HCenter()
                                .Margin(0, 28, 0, 0)
                                .Scale(State.ContentOpacity)
                                .WithAnimation(duration: 400, easing: Easing.SpringOut),

                            // Title
                            Label(title)
                                .FontSize(30)
                                .FontAttributes(FontAttributes.Bold)
                                .TextColor(AppColors.TextPrimary)
                                .HorizontalTextAlignment(TextAlignment.Center)
                                .LineHeight(1.3)
                                .Margin(28, 20, 28, 0),

                            // Body
                            Label(body)
                                .FontSize(15)
                                .TextColor(AppColors.TextSecondary)
                                .HorizontalTextAlignment(TextAlignment.Center)
                                .LineHeight(1.6)
                                .Margin(36, 12, 36, 0),

                            // Accent separator
                            BoxView()
                                .Color(accent.WithAlpha(0.25f))
                                .WidthRequest(60)
                                .HeightRequest(2)
                                .CornerRadius(1)
                                .HCenter()
                                .Margin(0, 20, 0, 0),

                            // ── Preference step (last step only) ───────────────
                            isLast ? RenderPreferences(accent) : null
                        )
                        .Spacing(0)
                        .Opacity(State.ContentOpacity)
                        .WithAnimation(duration: 300, easing: Easing.CubicOut)
                    )
                )
                .GridRow(0),

                // ────────────────── Navigation controls ─────────────────────────
                VStack(
                    // Dot progress indicators
                    HStack(
                        [.. Steps.Select((_, i) => (VisualNode)
                            BoxView()
                                .WidthRequest(i == State.CurrentStep ? 28 : 8)
                                .HeightRequest(8)
                                .CornerRadius(4)
                                .Color(i == State.CurrentStep ? accent : AppColors.TextMuted.WithAlpha(0.4f))
                                .Margin(3, 0)
                                .WithAnimation(duration: 300, easing: Easing.CubicOut)
                        )]
                    )
                    .HCenter()
                    .Margin(0, 0, 0, 20),

                    // Primary CTA button
                    Border(
                        Label(isLast
                                ? (State.IsSaving ? "Setting up…" : "Get Started →")
                                : "Continue →")
                            .FontSize(16)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(AppColors.Background)
                            .HCenter()
                            .VCenter()
                    )
                    .StrokeShape(new RoundRectangle().CornerRadius(16))
                    .BackgroundColor(accent)
                    .HeightRequest(56)
                    .HFill()
                    .Margin(24, 0)
                    .IsEnabled(!State.IsSaving)
                    .OnTapped(isLast ? SaveAndContinue : NextStep),

                    // Back link
                    State.CurrentStep > 0
                        ? Label("← Back")
                            .FontSize(14)
                            .TextColor(AppColors.TextSecondary)
                            .HCenter()
                            .Margin(0, 12, 0, 0)
                            .OnTapped(PrevStep)
                        : BoxView().HeightRequest(12).Color(Colors.Transparent)
                )
                .GridRow(1)
                .Margin(0, 0, 0, 40)
            )
        )
        .BackgroundColor(AppColors.Background);
    }

    // ── Preferences step UI ───────────────────────────────────────────────────

    private VisualNode RenderPreferences(Color accent) =>
        VStack(
            Label("Colour Theme")
                .FontSize(13)
                .FontAttributes(FontAttributes.Bold)
                .TextColor(AppColors.TextSecondary)
                .CharacterSpacing(0.8)
                .HCenter()
                .Margin(0, 24, 0, 12),

            HStack(
                new[] { ("☀️ Light", "light"), ("🌙 Dark", "dark"), ("📱 System", "system") }
                    .Select(t => (VisualNode)
                        Border(
                            VStack(
                                Label(t.Item1.Split(' ')[0]).FontSize(22).HCenter(),
                                Label(t.Item1.Split(' ')[1])
                                    .FontSize(12)
                                    .FontAttributes(State.Theme == t.Item2
                                        ? FontAttributes.Bold : FontAttributes.None)
                                    .TextColor(State.Theme == t.Item2
                                        ? AppColors.Background : AppColors.TextSecondary)
                                    .HCenter()
                            )
                            .Spacing(4)
                            .Padding(0, 10)
                        )
                        .StrokeShape(new RoundRectangle().CornerRadius(14))
                        .BackgroundColor(State.Theme == t.Item2 ? accent : AppColors.Card)
                        .Stroke(State.Theme == t.Item2 ? accent : AppColors.CardBorder)
                        .StrokeThickness(State.Theme == t.Item2 ? 2 : 1)
                        .WidthRequest(96)
                        .HeightRequest(72)
                        .OnTapped(() => SetState(s => s.Theme = t.Item2))
                    ).ToArray()
            )
            .HCenter()
            .Spacing(10)
            .Margin(0, 0, 0, 8)
        )
        .Spacing(0);

    // ── Navigation handlers ───────────────────────────────────────────────────

    private async void NextStep()
    {
        SetState(s => s.ContentOpacity = 0.0);
        await Task.Delay(150);
        SetState(s =>
        {
            s.CurrentStep++;
            s.ContentOpacity = 1.0;
        });
    }

    private async void PrevStep()
    {
        SetState(s => s.ContentOpacity = 0.0);
        await Task.Delay(150);
        SetState(s =>
        {
            s.CurrentStep--;
            s.ContentOpacity = 1.0;
        });
    }

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
