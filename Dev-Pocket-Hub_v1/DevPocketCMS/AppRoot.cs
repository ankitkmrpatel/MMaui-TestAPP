using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;
using DevPocketCMS.UI.Pages;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS;

// ── Animated loading screen shown while the database initialises ──────────────

class SplashState
{
    public double LogoScale   { get; set; } = 0.6;
    public double LogoOpacity { get; set; } = 0.0;
    public double TagOpacity  { get; set; } = 0.0;
}

partial class AnimatedSplash : Component<SplashState>
{
    protected override async void OnMounted()
    {
        await Task.Delay(80);
        SetState(s =>
        {
            s.LogoScale   = 1.0;
            s.LogoOpacity = 1.0;
        });

        await Task.Delay(350);
        SetState(s => s.TagOpacity = 1.0);
    }

    public override VisualNode Render() =>
        ContentPage(
            VStack(
                // App icon glyph
                Border(
                    Label("📚")
                        .FontSize(56)
                        .HCenter()
                        .VCenter()
                )
                .StrokeShape(new MauiReactor.Shapes.RoundRectangle().CornerRadius(28))
                .BackgroundColor(AppColors.Card)
                .Stroke(AppColors.Accent)
                .StrokeThickness(1.5)
                .WidthRequest(110)
                .HeightRequest(110)
                .HCenter()
                .Scale(State.LogoScale)
                .Opacity(State.LogoOpacity)
                .WithAnimation(duration: 600, easing: Easing.SpringOut),

                // App name
                Label("DevPocketCMS")
                    .FontSize(26)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(AppColors.TextPrimary)
                    .HCenter()
                    .Margin(0, 20, 0, 4)
                    .Opacity(State.LogoOpacity)
                    .WithAnimation(duration: 500, easing: Easing.CubicOut),

                // Tagline
                Label("Your developer reading hub")
                    .FontSize(14)
                    .TextColor(AppColors.TextMuted)
                    .HCenter()
                    .Opacity(State.TagOpacity)
                    .WithAnimation(duration: 400, easing: Easing.CubicOut),

                // Loading indicator
                ActivityIndicator()
                    .IsRunning(true)
                    .Color(AppColors.Accent)
                    .HCenter()
                    .Margin(0, 48, 0, 0)
                    .Opacity(State.TagOpacity)
                    .WithAnimation(duration: 400, easing: Easing.CubicOut)
            )
            .Spacing(0)
            .VCenter()
        )
        .BackgroundColor(AppColors.Background);
}

// ── Root component ────────────────────────────────────────────────────────────

class AppRootState
{
    public bool IsInitialized  { get; set; }
    public bool ShowOnboarding { get; set; }
}

partial class AppRoot : Component<AppRootState>
{
    private static bool _dbInitialized;

    [Inject] readonly IPreferencesRepository _prefs;
    [Inject] readonly AppDatabase _db;

    protected override async void OnMounted()
    {
        if (!_dbInitialized)
        {
            await _db.InitializeAsync();
            _dbInitialized = true;
        }

        var prefs = await _prefs.GetAsync();
        SetState(s =>
        {
            s.ShowOnboarding = !prefs.HasCompletedOnboarding;
            s.IsInitialized  = true;
        });
    }

    public override VisualNode Render()
    {
        if (!State.IsInitialized)
            return new AnimatedSplash();

        if (State.ShowOnboarding)
            return new OnboardingPage()
                .OnComplete(() => SetState(s => s.ShowOnboarding = false));

        return new MainShell();
    }
}
