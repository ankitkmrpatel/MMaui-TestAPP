using DevPocketCMS.Application.Services;
using DevPocketCMS.Core.Entities;
using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Pages;

class SettingsState
{
    public UserPreferences? Prefs { get; set; }
    public bool IsLoading { get; set; } = true;
    public bool IsSaving { get; set; }
    public string? StatusMsg { get; set; }
}

partial class SettingsPage : Component<SettingsState>
{
    [Inject] IPreferencesRepository _prefs;
    [Inject] INotificationService _notifications;
    [Inject] ArticleService _articles;

    protected override async void OnMounted() => await LoadAsync();

    private async Task LoadAsync()
    {
        var p = await _prefs.GetAsync();
        SetState(s => { s.Prefs = p; s.IsLoading = false; });
    }

    public override VisualNode Render() =>
        ContentPage(
            ScrollView(
                VStack(
                    Label("Settings")
                        .FontSize(26)
                        .FontAttributes(FontAttributes.Bold)
                        .TextColor(AppColors.TextPrimary)
                        .Margin(20, 16, 20, 4),

                    State.IsLoading || State.Prefs is null
                        ? ActivityIndicator().IsRunning(true).Color(AppColors.Accent).HCenter().Margin(0, 32)
                        : RenderSettings(State.Prefs),

                    State.StatusMsg is not null
                        ? Label(State.StatusMsg)
                            .FontSize(13)
                            .TextColor(AppColors.Accent)
                            .HCenter()
                            .Margin(20, 8)
                        : null
                )
                .Spacing(0)
            )
#warning 1,1,1 CHeck this Issue
        //.ContentInsetAdjustmentBehavior(ScrollViewContentInsetAdjustmentBehavior.Always)
        )
        .BackgroundColor(AppColors.Background)
        .Title(string.Empty);

    private VisualNode RenderSettings(UserPreferences p) =>
        VStack(
            // ── Profile section ───────────────────────────────────────────
            SectionLabel("Profile"),
            SettingRow("User Type", p.UserType),

            // ── Theme ─────────────────────────────────────────────────────
            SectionLabel("Appearance"),
            Border(
                VStack(
                    Label("Theme").FontSize(15).TextColor(AppColors.TextPrimary),
                    HStack(
                        ThemeButton("Light", "light", p),
                        ThemeButton("Dark", "dark", p),
                        ThemeButton("System", "system", p)
                    )
                    .Spacing(8)
                )
                .Spacing(10)
                .Padding(16)
            )
            .StrokeShape(new RoundRectangle().CornerRadius(14))
            .BackgroundColor(AppColors.Card)
            .Stroke(AppColors.CardBorder)
            .StrokeThickness(1)
            .Margin(20, 4),

            // ── Notifications ─────────────────────────────────────────────
            SectionLabel("Notifications"),
            Border(
                VStack(
                    HStack(
                        Label("Daily Reminder")
                            .FontSize(15).TextColor(AppColors.TextPrimary).HFill(),
                        Switch()
                            .IsToggled(p.NotificationsEnabled)
                            .OnColor(AppColors.Accent)
                            .OnToggled(async (e) =>
                            {
                                var v = e.Value;
                                p.NotificationsEnabled = v;
                                await SaveAsync(p);
                                if (v)
                                {
                                    bool granted = await _notifications.RequestPermissionAsync();
                                    if (granted)
                                    {
                                        var unread = await _articles.GetUnreadCountAsync();
                                        await _notifications.ScheduleDailyReminderAsync(
                                            TimeSpan.Parse(p.NotificationTime ?? "09:00"), unread);
                                        ShowStatus("Daily reminder set!");
                                    }
                                    else
                                    {
                                        ShowStatus("Notification permission denied.");
                                        p.NotificationsEnabled = false;
                                        await SaveAsync(p);
                                    }
                                }
                                else
                                {
                                    await _notifications.CancelAllAsync();
                                }
                            })
                    ),

                    p.NotificationsEnabled
                        ? VStack(
                            Label("Reminder Time")
                                .FontSize(13).TextColor(AppColors.TextSecondary),
                            Entry()
                                .Text(p.NotificationTime ?? "09:00")
                                .Placeholder("HH:mm")
                                .PlaceholderColor(AppColors.TextMuted)
                                .TextColor(AppColors.TextPrimary)
                                .Keyboard(Keyboard.Numeric)
                                .OnTextChanged(t =>
                                {
                                    p.NotificationTime = t;
                                    _ = SaveAsync(p);
                                })
                          )
                          .Spacing(4)
                        : null
                )
                .Spacing(12)
                .Padding(16)
            )
            .StrokeShape(new RoundRectangle().CornerRadius(14))
            .BackgroundColor(AppColors.Card)
            .Stroke(AppColors.CardBorder)
            .StrokeThickness(1)
            .Margin(20, 4),

            // ── About ─────────────────────────────────────────────────────
            SectionLabel("About"),
            Border(
                VStack(
                    SettingRowInline("Version", "1.0.0"),
                    SettingRowInline("User Type", "Developer"),
                    SettingRowInline("Storage", "SQLite (local)")
                )
                .Spacing(0)
                .Padding(16)
            )
            .StrokeShape(new RoundRectangle().CornerRadius(14))
            .BackgroundColor(AppColors.Card)
            .Stroke(AppColors.CardBorder)
            .StrokeThickness(1)
            .Margin(20, 4, 20, 32)
        )
        .Spacing(0);

    private VisualNode ThemeButton(string label, string value, UserPreferences p) =>
        Button(label)
            .BackgroundColor(p.Theme == value ? AppColors.Accent : AppColors.Surface)
            .TextColor(p.Theme == value ? AppColors.Background : AppColors.TextSecondary)
            .CornerRadius(10)
            .HeightRequest(40)
            .HFill()
            .OnClicked(async () =>
            {
                p.Theme = value;
                await SaveAsync(p);
                ApplyTheme(value);
            });

    private static VisualNode SectionLabel(string text) =>
        Label(text)
            .FontSize(11)
            .FontAttributes(FontAttributes.Bold)
            .TextColor(AppColors.TextMuted)
            .CharacterSpacing(1.2)
            .Margin(20, 16, 20, 4);

    private static VisualNode SettingRow(string key, string value) =>
        Border(
            Grid(rows: "*", columns: "*, Auto",
                Label(key).FontSize(15).TextColor(AppColors.TextPrimary).VCenter(),
                Label(value).FontSize(14).TextColor(AppColors.TextSecondary).GridColumn(1).VCenter()
            )
            .Padding(16, 14)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .Margin(20, 4);

    private static VisualNode SettingRowInline(string key, string value) =>
        Grid(rows: "*", columns: "*, Auto",
            Label(key).FontSize(14).TextColor(AppColors.TextSecondary).VCenter(),
            Label(value).FontSize(14).FontAttributes(FontAttributes.Bold)
                .TextColor(AppColors.TextPrimary).GridColumn(1).VCenter()
        )
        .Padding(0, 6);

    private async Task SaveAsync(UserPreferences p)
    {
        await _prefs.SaveAsync(p);
        SetState(s => s.Prefs = p);
    }

    private void ShowStatus(string msg)
    {
        SetState(s => s.StatusMsg = msg);
        Task.Delay(2500).ContinueWith(_ => SetState(s => s.StatusMsg = null));
    }

    private static void ApplyTheme(string theme)
    {
        if (Microsoft.Maui.Controls.Application.Current is null) return;
        Microsoft.Maui.Controls.Application.Current.UserAppTheme = theme switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
