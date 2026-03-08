using DevPocketCMS.UI.Theme;
using MauiReactor;

namespace DevPocketCMS.UI.Components;

class EmptyState : Component
{
    private string _icon = "📄";
    private string _message = "Nothing here yet.";

    public EmptyState Icon(string icon) { _icon = icon; return this; }
    public EmptyState Message(string message) { _message = message; return this; }

    public override VisualNode Render() =>
        VStack(
            Label(_icon).FontSize(52).HCenter(),
            Label(_message)
                .FontSize(15)
                .TextColor(AppColors.TextMuted)
                .HorizontalTextAlignment(TextAlignment.Center)
                .HCenter()
                .Margin(32, 8)
        )
        .Spacing(12)
        .HCenter()
        .VCenter()
        .Margin(0, 60);
}
