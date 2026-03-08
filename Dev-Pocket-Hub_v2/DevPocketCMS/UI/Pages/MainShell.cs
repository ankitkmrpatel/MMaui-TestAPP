using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

// ─────────────────────────────────────────────────────────────────────────────
// Tab definitions
// ─────────────────────────────────────────────────────────────────────────────

record TabDef(string Icon, string Label);

// ─────────────────────────────────────────────────────────────────────────────
// State
// ─────────────────────────────────────────────────────────────────────────────

class MainShellState
{
    public int SelectedTab { get; set; } = 0;
}

// ─────────────────────────────────────────────────────────────────────────────
// Component
// ─────────────────────────────────────────────────────────────────────────────

partial class MainShell : Component<MainShellState>
{
    // Tab 0 → Dashboard, Tab 3 → Feed  (reversed from original order)
    private static readonly TabDef[] Tabs =
    [
        new("🏠", "Home"),
        new("🗂️", "Lists"),
        new("🔍", "Search"),
        new("📰", "Feed"),
        new("⚙️", "Settings"),
    ];

    public override VisualNode Render() =>
        ContentPage(
            Grid(rows: "*, Auto", columns: "*",

                // ── Page content ─────────────────────────────────────────────
                RenderPage(State.SelectedTab)
                    .GridRow(0),

                // ── Animated bottom tab bar ───────────────────────────────────
                RenderTabBar()
                    .GridRow(1)
            )
        )
        .BackgroundColor(AppColors.Background);

    // ── Page router ──────────────────────────────────────────────────────────

    private VisualNode RenderPage(int tab) => tab switch
    {
        0 => new DashboardPage(),
        1 => new ListsPage(),
        2 => new SearchPage(),
        3 => new HomePage(),
        4 => new SettingsPage(),
        _ => new DashboardPage(),
    };

    // ── Tab bar ───────────────────────────────────────────────────────────────

    private VisualNode RenderTabBar() =>
        Border(
            Grid(rows: "Auto", columns: "*, *, *, *, *",
                Tabs.Select((tab, i) => TabButton(tab.Icon, i).GridColumn(i)).ToArray()
            )
            .Padding(0, 8, 0, DeviceInfo.Platform == DevicePlatform.iOS ? 24 : 10)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(new CornerRadius(20, 20, 0, 0)))
        .BackgroundColor(AppColors.Surface)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1);

    private VisualNode TabButton(string icon, int idx)
    {
        bool isSelected = State.SelectedTab == idx;

        return VStack(
            // Icon
            Label(icon)
                .FontSize(24)
                .HCenter()
                .Scale(isSelected ? 1.15 : 1.0)
                .Opacity(isSelected ? 1.0 : 0.38)
                .WithAnimation(duration: 220, easing: Easing.SpringOut),

            // Animated indicator bar
            BoxView()
                .Color(AppColors.Accent)
                .CornerRadius(2)
                .HeightRequest(3)
                .WidthRequest(isSelected ? 22 : 0)
                .HCenter()
                .WithAnimation(duration: 220, easing: Easing.SpringOut)
        )
        .Spacing(5)
        .HFill()
        .Padding(0, 6)
        .OnTapped(() => SetState(s => s.SelectedTab = idx));
    }
}
