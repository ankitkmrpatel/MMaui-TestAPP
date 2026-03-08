using DevPocketCMS.UI.Theme;
using MauiReactor;
using ShellContent = MauiReactor.ShellContent;
using Tab = MauiReactor.Tab;
using TabBar = MauiReactor.TabBar;

namespace DevPocketCMS.UI.Pages;

/// <summary>
/// Tab-based Shell that acts as the main navigation host.
/// Five tabs: Feed · Lists · Search · Dashboard · Settings.
/// </summary>
class MainShell : Component
{
    public override VisualNode Render() =>
        Shell(
            new TabBar()
            {
                new Tab("Feed", "feed.png")
                {
                    new ShellContent()
                        .Title("Feed")
                        .RenderContent(() => new HomePage())
                },
                new Tab("Lists", "lists.png")
                {
                    new ShellContent()
                        .Title("Lists")
                        .RenderContent(() => new ListsPage())
                },
                new Tab("Search", "search.png")
                {
                    new ShellContent()
                        .Title("Search")
                        .RenderContent(() => new SearchPage())
                },
                new Tab("Dashboard", "dashboard.png")
                {
                    new ShellContent()
                        .Title("Dashboard")
                        .RenderContent(() => new DashboardPage())
                },
                new Tab("Settings", "settings.png")
                {
                    new ShellContent()
                        .Title("Settings")
                        .RenderContent(() => new SettingsPage())
                },
            }
        )
        .Set(Microsoft.Maui.Controls.Shell.TabBarBackgroundColorProperty, AppColors.Surface)
        .Set(Microsoft.Maui.Controls.Shell.TabBarForegroundColorProperty, AppColors.Accent)
        .Set(Microsoft.Maui.Controls.Shell.TabBarUnselectedColorProperty, AppColors.TextMuted)
        .Set(Microsoft.Maui.Controls.Shell.BackgroundColorProperty, AppColors.Background);
}
