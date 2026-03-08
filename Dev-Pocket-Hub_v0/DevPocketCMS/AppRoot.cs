using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;
using DevPocketCMS.UI.Pages;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Animations;

namespace DevPocketCMS;

/// <summary>
/// Root component: initialises preferences and routes to
/// OnboardingPage (first run) or MainShell (returning user).
/// </summary>
class AppRootState
{
    public bool IsInitialized    { get; set; }
    public bool ShowOnboarding   { get; set; }
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
            // Initialize Database
            await _db.InitializeAsync();
            _dbInitialized = true;
        }

        // Load StartUp Preferences
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
        {
            return ContentPage(
                ActivityIndicator()
                    .IsRunning(true)
                    .Color(AppColors.Accent)
                    .HCenter()
                    .VCenter()
            )
            .BackgroundColor(AppColors.Background);
        }

        if (State.ShowOnboarding)
        {
            return new OnboardingPage()
                .OnComplete(() => SetState(s => s.ShowOnboarding = false));
        }

        return new MainShell();
    }
}
