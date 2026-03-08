namespace DevPocketCMS.UI.Theme;

/// <summary>
/// Centralized color palette. All UI code uses these constants —
/// no hardcoded hex strings elsewhere.
/// </summary>
public static class AppColors
{
    // Backgrounds
    public static readonly Color Background = Color.FromArgb("#090912");
    public static readonly Color Surface = Color.FromArgb("#13131F");
    public static readonly Color Card = Color.FromArgb("#1A1A2E");
    public static readonly Color CardBorder = Color.FromArgb("#252540");

    // Text
    public static readonly Color TextPrimary = Color.FromArgb("#FFFFFF");
    public static readonly Color TextSecondary = Color.FromArgb("#8888BB");
    public static readonly Color TextMuted = Color.FromArgb("#55557A");

    // Accent
    public static readonly Color Accent = Color.FromArgb("#4FFFB0");
    public static readonly Color AccentDark = Color.FromArgb("#2DD68A");
    public static readonly Color AccentBg = Color.FromArgb("#1A2E26");

    // Status
    public static readonly Color Danger = Color.FromArgb("#FF4D6D");
    public static readonly Color Warning = Color.FromArgb("#FFB830");
    public static readonly Color Success = Color.FromArgb("#4FFFB0");

    // List palette
    public static readonly string[] ListColors =
    {
        "#4FFFB0", "#00C2FF", "#FF6B6B", "#FFD166", "#C77DFF", "#FF9A3C",
        "#44D7B6", "#F28B82", "#A8D8EA", "#FCC2D7",
    };

    // Separator
    public static readonly Color Separator = Color.FromArgb("#1E1E32");

    // Tag chip background
    public static readonly Color TagBg = Color.FromArgb("#1E1E3A");
}
